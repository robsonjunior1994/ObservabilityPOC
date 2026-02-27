using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObservabilityPOC.Api.Data;

namespace ObservabilityPOC.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public HealthController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var databaseHealthy = await _dbContext.Database.CanConnectAsync(cancellationToken);

        return Ok(new
        {
            status = databaseHealthy ? "Healthy" : "Degraded",
            database = databaseHealthy ? "Healthy" : "Unhealthy",
            timestamp = DateTime.UtcNow
        });
    }

    [Route("500")]
    [HttpGet]
    public async Task<IActionResult> GetStatusCode(int value)
    {
        //var databaseHealthy = await _dbContext.Database.CanConnectAsync(cancellationToken);

        //return StatusCode(StatusCodes.Status500InternalServerError, new
        //{
        //    status = databaseHealthy ? "Healthy" : "Degraded",
        //    database = databaseHealthy ? "Healthy" : "Unhealthy",
        //    timestamp = DateTime.UtcNow
        //});

        switch (value)
        {
            case 500:
                return StatusCode(StatusCodes.Status500InternalServerError);
            case 400:
                return BadRequest();
            case 404:
                return NotFound();
            default:
                break;
        }

        return Ok();


    }

}
