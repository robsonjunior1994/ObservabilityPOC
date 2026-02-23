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

    public TicketsController(ITicketService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TicketCreateRequest request, CancellationToken cancellationToken)
    {
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TicketUpdateRequest request, CancellationToken cancellationToken)
    {
        var response = await _service.UpdateAsync(id, request, cancellationToken);
        return ToActionResult(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
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
            _ => StatusCode(StatusCodes.Status500InternalServerError, response)
        };
    }
}
