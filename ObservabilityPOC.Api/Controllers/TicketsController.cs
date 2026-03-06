using Microsoft.AspNetCore.Mvc;
using ObservabilityPOC.Api.Requests;
using ObservabilityPOC.Api.Responses;
using ObservabilityPOC.Api.Services;

namespace ObservabilityPOC.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _service;
    private readonly ILogger<TicketsController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public TicketsController(ITicketService service, ILogger<TicketsController> logger, IHttpClientFactory httpClientFactory)
    {
        _service = service;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TicketCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating ticket {@Request}", request);
        var response = await _service.CreateAsync(request, cancellationToken);
        return ToActionResult(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _service.GetAllAsync(cancellationToken);
        return ToActionResult(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _service.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    [HttpGet("httpbin")]
    public async Task<IActionResult> CallHttpBin(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();

        try
        {
            _logger.LogInformation("Calling httpbin GET endpoint");
            using var response = await client.GetAsync("https://httpbin.org/status/500", cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("httpbin call failed with status {StatusCode}. Body: {Body}", (int)response.StatusCode, body);
                return StatusCode((int)response.StatusCode, new { error = "httpbin call failed", body });
            }

            return Ok(body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "httpbin call threw an exception");
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "httpbin call failed" });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TicketUpdateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating ticket {TicketId} {@Request}", id, request);
        var response = await _service.UpdateAsync(id, request, cancellationToken);
        return ToActionResult(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting ticket {TicketId}", id);
        var response = await _service.DeleteAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    private IActionResult ToActionResult<T>(ApiResponse<T> response)
    {
        if (string.IsNullOrWhiteSpace(response.ErrorCode))
        {
            return Ok(response);
        }

        return response.ErrorCode switch
        {
            "Validation" => BadRequest(response),
            "NotFound" => NotFound(response),
            "Database" => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response)
        };
    }
}
