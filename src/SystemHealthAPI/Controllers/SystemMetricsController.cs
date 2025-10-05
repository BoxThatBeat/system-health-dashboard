using Microsoft.AspNetCore.Mvc;
using SystemHealthAPI.Models;
using SystemHealthAPI.Services;

namespace SystemHealthAPI.Controllers;

/// <summary>
/// Provides endpoints for retrieving and adding OS system metrics.
/// </summary>
[ApiController]
[Route("[controller]")]
public class SystemMetricsController : ControllerBase
{
    private readonly ISystemMetricsService _systemMetricsService;

    /// <summary>
    /// Creates a new SystemMetricsController with the injected services.
    /// </summary>
    /// <param name="systemMetricsService">The systemMetricsService</param>
    public SystemMetricsController(ISystemMetricsService systemMetricsService)
    {
        _systemMetricsService = systemMetricsService;
    }

    /// <summary>
    /// Retrieves all operating system metrics.
    /// </summary>
    /// <returns>A list of <see cref="OSMetric"/></returns>
    [HttpGet]
    public async Task<IEnumerable<OSMetric>> GetAll()
    {
        return await _systemMetricsService.RetrieveAllMetricsAsync();
    }

    /// <summary>
    /// Adds a list of operating system metrics.
    /// </summary>
    /// <param name="metrics">A list of <see cref="OSMetric"/></param>
    /// <returns><see cref="IActionResult"/></returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] IEnumerable<OSMetric> metrics)
    {
        if (metrics == null || !metrics.Any())
        {
            return BadRequest("Metrics list cannot be null or empty.");
        }

        bool result = await _systemMetricsService.AddMetricsAsync(metrics);

        if (result)
        {
            return Ok("Metrics added successfully.");
        }
        else
        {
            return StatusCode(500, "An unexpected error occurred while adding metrics.");
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAll()
    {
        bool result = await _systemMetricsService.ClearAllMetricsAsync();
        if (result)
        {
            return NoContent();
        }
        else
        {
            return StatusCode(500, "An unexpected error occurred while deleting metrics.");
        }
    }
}