using ObservabilityPOC.Api.Models;

namespace ObservabilityPOC.Api.Repositories;

public interface ITicketRepository
{
    Task<int> CreateAsync(Ticket ticket, CancellationToken cancellationToken);
    Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Ticket ticket, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
