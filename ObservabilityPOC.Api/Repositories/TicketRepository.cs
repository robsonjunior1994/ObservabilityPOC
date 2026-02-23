using Microsoft.EntityFrameworkCore;
using ObservabilityPOC.Api.Data;
using ObservabilityPOC.Api.Models;

namespace ObservabilityPOC.Api.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<TicketRepository> _logger;

    public TicketRepository(AppDbContext context, ILogger<TicketRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> CreateAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating ticket for {Email}", ticket.Email);

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        return ticket.Id;
    }

    public async Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting ticket {TicketId}", id);

        return await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(ticket => ticket.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing tickets");

        return await _context.Tickets
            .AsNoTracking()
            .OrderByDescending(ticket => ticket.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating ticket {TicketId}", ticket.Id);

        _context.Tickets.Update(ticket);
        var rows = await _context.SaveChangesAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting ticket {TicketId}", id);

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (ticket is null)
        {
            return false;
        }

        _context.Tickets.Remove(ticket);
        var rows = await _context.SaveChangesAsync(cancellationToken);
        return rows > 0;
    }
}
