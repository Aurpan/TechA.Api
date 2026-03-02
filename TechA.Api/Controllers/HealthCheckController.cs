using Microsoft.AspNetCore.Mvc;
using TechA.Core.Enums;
using TechA.Core.Interfaces.Service;

namespace TechA.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
public class HealthCheckController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;

    public HealthCheckController(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet("live")]
    public IActionResult Live() => Ok();


    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        var response = await _healthCheckService.GetReadinessAsync();
        
        if (response.Status != ConnectivityStatus.Healthy)
        {
            return StatusCode(503, new { Status = response.Status.ToString(), response.Checks });
        }

        return Ok(new { Status = response.Status.ToString(), response.Checks });
    }
}
