namespace ObservabilityPOC.Api.Responses;

public class ApiResponse<T>
{
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data) => new() { Data = data };

    public static ApiResponse<T> Failure(string errorCode, string errorMessage) => new()
    {
        ErrorCode = errorCode,
        ErrorMessage = errorMessage
    };
}
