using ObservabilityPOC.Api.Models;
using ObservabilityPOC.Api.Repositories;
using ObservabilityPOC.Api.Requests;
using ObservabilityPOC.Api.Responses;

namespace ObservabilityPOC.Api.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _repository;
    private readonly ILogger<TicketService> _logger;

    public TicketService(ITicketRepository repository, ILogger<TicketService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ApiResponse<TicketResponse>> CreateAsync(TicketCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service creating ticket for {Email}", request.Email);

        var validation = ValidateRequest(request.Email, request.Address, request.Occurred);
        if (validation is not null)
        {
            return ApiResponse<TicketResponse>.Failure("Validation", validation);
        }

        var ticket = new Ticket
        {
            Email = request.Email.Trim(),
            Address = request.Address.Trim(),
            Occurred = request.Occurred.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var id = await _repository.CreateAsync(ticket, cancellationToken);
        ticket.Id = id;

        return ApiResponse<TicketResponse>.Success(Map(ticket));
    }

    public async Task<ApiResponse<TicketResponse?>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service getting ticket {TicketId}", id);

        var ticket = await _repository.GetByIdAsync(id, cancellationToken);
        if (ticket is null)
        {
            return ApiResponse<TicketResponse?>.Failure("NotFound", "Chamado não encontrado.");
        }

        return ApiResponse<TicketResponse?>.Success(Map(ticket));
    }

    public async Task<ApiResponse<IReadOnlyList<TicketResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service listing tickets");

        var tickets = await _repository.GetAllAsync(cancellationToken);
        var response = tickets.Select(Map).ToList();

        return ApiResponse<IReadOnlyList<TicketResponse>>.Success(response);
    }

    public async Task<ApiResponse<TicketResponse?>> UpdateAsync(int id, TicketUpdateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service updating ticket {TicketId}", id);

        var validation = ValidateRequest(request.Email, request.Address, request.Occurred);
        if (validation is not null)
        {
            return ApiResponse<TicketResponse?>.Failure("Validation", validation);
        }

        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ApiResponse<TicketResponse?>.Failure("NotFound", "Chamado não encontrado.");
        }

        existing.Email = request.Email.Trim();
        existing.Address = request.Address.Trim();
        existing.Occurred = request.Occurred.Trim();

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        if (!updated)
        {
            return ApiResponse<TicketResponse?>.Failure("Database", "Não foi possível atualizar o chamado.");
        }

        return ApiResponse<TicketResponse?>.Success(Map(existing));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service deleting ticket {TicketId}", id);

        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return ApiResponse<bool>.Failure("NotFound", "Chamado não encontrado.");
        }

        return ApiResponse<bool>.Success(true);
    }

    private static string? ValidateRequest(string email, string address, string occurred)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(occurred))
        {
            return "Email, endereço e ocorrido são obrigatórios.";
        }

        return null;
    }

    private static TicketResponse Map(Ticket ticket)
    {
        return new TicketResponse
        {
            Id = ticket.Id,
            Email = ticket.Email,
            Address = ticket.Address,
            Occurred = ticket.Occurred,
            CreatedAt = ticket.CreatedAt
        };
    }
}
