using ObservabilityPOC.Api.Requests;
using ObservabilityPOC.Api.Responses;

namespace ObservabilityPOC.Api.Services;

public interface ITicketService
{
    Task<ApiResponse<TicketResponse>> CreateAsync(TicketCreateRequest request, CancellationToken cancellationToken);
    Task<ApiResponse<TicketResponse?>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ApiResponse<IReadOnlyList<TicketResponse>>> GetAllAsync(CancellationToken cancellationToken);
    Task<ApiResponse<TicketResponse?>> UpdateAsync(int id, TicketUpdateRequest request, CancellationToken cancellationToken);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken);
}
