using Credito.ScoreEngine.API.Models;
using Credito.ScoreEngine.API.Services;
using Credito.ScoreEngine.API.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Credito.ScoreEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoringController : ControllerBase
{
    private readonly ExternalServiceExecutor _serviceExecutor;
    private readonly ILogger<ScoringController> _logger;

    public ScoringController(
        ExternalServiceExecutor serviceExecutor,
        ILogger<ScoringController> logger)
    {
        _serviceExecutor = serviceExecutor;
        _logger = logger;
    }

    [HttpPost("execute")]
    public async Task<ActionResult<ServiceResponse>> ExecuteService(
        [FromBody] ServiceRequest request)
    {
        try
        {
            var result = await _serviceExecutor.ExecuteServiceAsync(request);
            return Ok(result);
        }
        catch (ServiceUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request for service {ServiceName}", 
                request.ServiceName);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}