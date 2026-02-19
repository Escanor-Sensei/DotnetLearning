using Microsoft.AspNetCore.Mvc;

namespace TaskManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        
        _logger.LogInformation("Health check called with correlation ID: {CorrelationId}", correlationId);
        
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            correlationId = correlationId,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Endpoint that simulates a slow operation (for performance testing)
    /// </summary>
    [HttpGet("slow")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSlowHealth()
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        
        _logger.LogInformation("Slow health check started with correlation ID: {CorrelationId}", correlationId);
        
        // Simulate slow operation
        await Task.Delay(3000); // 3 seconds delay
        
        _logger.LogInformation("Slow health check completed with correlation ID: {CorrelationId}", correlationId);
        
        return Ok(new
        {
            status = "healthy_slow",
            processingTime = "3000ms",
            timestamp = DateTime.UtcNow,
            correlationId = correlationId
        });
    }

    /// <summary>
    /// Endpoint that throws an exception (for exception handling testing)
    /// </summary>
    [HttpGet("error")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetError()
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        
        _logger.LogInformation("Error endpoint called with correlation ID: {CorrelationId}", correlationId);
        
        throw new InvalidOperationException("This is a test exception to demonstrate error handling middleware");
    }

    /// <summary>
    /// Endpoint for rate limiting testing
    /// </summary>
    [HttpGet("load-test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult GetLoadTest()
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        
        return Ok(new
        {
            message = "Load test endpoint",
            correlationId = correlationId,
            timestamp = DateTime.UtcNow,
            tip = "Call this endpoint repeatedly to test rate limiting"
        });
    }
}