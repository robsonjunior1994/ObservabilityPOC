using System.Diagnostics;

namespace ObservabilityPOC.Api.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startedAt = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        var requestBody = await ReadRequestBodyAsync(context.Request, context.RequestAborted);

        var originalBodyStream = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var finishedAt = DateTime.UtcNow;
            var responseBodyText = await ReadResponseBodyAsync(context.Response, context.RequestAborted);

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms (Started: {StartedAt}, Finished: {FinishedAt})\nRequestBody: {RequestBody}\nResponseBody: {ResponseBody}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                startedAt,
                finishedAt,
                requestBody,
                responseBodyText);

            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream, context.RequestAborted);
            context.Response.Body = originalBodyStream;
        }
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (request.ContentLength is null || request.ContentLength == 0)
        {
            return string.Empty;
        }

        request.EnableBuffering();
        request.Body.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync(cancellationToken);

        request.Body.Seek(0, SeekOrigin.Begin);
        return body;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpResponse response, CancellationToken cancellationToken)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(response.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync(cancellationToken);
        response.Body.Seek(0, SeekOrigin.Begin);
        return body;
    }
}
