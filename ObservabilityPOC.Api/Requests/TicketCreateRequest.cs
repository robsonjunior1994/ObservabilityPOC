namespace ObservabilityPOC.Api.Requests;

public class TicketCreateRequest
{
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Occurred { get; set; } = string.Empty;
}
