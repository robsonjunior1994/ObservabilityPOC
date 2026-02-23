namespace ObservabilityPOC.Api.Responses;

public class TicketResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Occurred { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
