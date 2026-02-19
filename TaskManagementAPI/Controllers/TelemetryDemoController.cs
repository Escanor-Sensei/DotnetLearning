using Microsoft.AspNetCore.Mvc;
using Microsoft.ApplicationInsights;
using TaskManagementAPI.Services;

namespace TaskManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelemetryDemoController : ControllerBase
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ITelemetryService _telemetryService;
    private readonly ILogger<TelemetryDemoController> _logger;

    public TelemetryDemoController(
        TelemetryClient telemetryClient, 
        ITelemetryService telemetryService,
        ILogger<TelemetryDemoController> logger)
    {
        _telemetryClient = telemetryClient;
        _telemetryService = telemetryService;
        _logger = logger;
    }

    /// <summary>
    /// Demonstrates custom event tracking
    /// </summary>
    [HttpGet("custom-event")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult TrackCustomEvent()
    {
        // Track a custom event with properties
        _telemetryClient.TrackEvent("DemoEvent", new Dictionary<string, string>
        {
            { "Source", "TelemetryDemoController" },
            { "Feature", "CustomEventTracking" },
            { "Timestamp", DateTime.UtcNow.ToString("O") }
        });

        _logger.LogInformation("Custom event tracked");

        return Ok(new
        {
            message = "Custom event tracked successfully",
            eventName = "DemoEvent",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Demonstrates custom metric tracking
    /// </summary>
    [HttpGet("custom-metric")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult TrackCustomMetric()
    {
        var randomValue = Random.Shared.Next(1, 100);
        
        // Track custom metric
        _telemetryClient.TrackMetric("DemoMetric", randomValue);
        _telemetryClient.TrackMetric("API_Usage", 1);

        return Ok(new
        {
            message = "Custom metric tracked successfully",
            metricName = "DemoMetric",
            value = randomValue,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Demonstrates dependency tracking
    /// </summary>
    [HttpGet("dependency-tracking")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TrackDependency()
    {
        // Simulate external dependency call
        using var dependencyTelemetry = _telemetryClient.StartOperation<Microsoft.ApplicationInsights.DataContracts.DependencyTelemetry>("ExternalAPI");
        dependencyTelemetry.Telemetry.Type = "HTTP";
        dependencyTelemetry.Telemetry.Target = "api.example.com";
        dependencyTelemetry.Telemetry.Data = "GET /api/data";

        try
        {
            // Simulate API call delay
            await Task.Delay(Random.Shared.Next(100, 500));
            
            dependencyTelemetry.Telemetry.Success = true;
            dependencyTelemetry.Telemetry.ResultCode = "200";

            return Ok(new
            {
                message = "Dependency call tracked successfully",
                duration = $"{dependencyTelemetry.Telemetry.Duration.TotalMilliseconds}ms",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            dependencyTelemetry.Telemetry.Success = false;
            _telemetryClient.TrackException(ex);
            throw;
        }
    }

    /// <summary>
    /// Demonstrates exception tracking
    /// </summary>
    [HttpGet("track-exception")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult TrackException()
    {
        try
        {
            // Simulate an exception
            throw new InvalidOperationException("This is a demo exception for Application Insights");
        }
        catch (Exception ex)
        {
            // Track exception with custom properties
            _telemetryClient.TrackException(ex, new Dictionary<string, string>
            {
                { "DemoType", "IntentionalException" },
                { "Controller", "TelemetryDemoController" },
                { "Severity", "Low" }
            });

            _logger.LogError(ex, "Demo exception occurred");

            return StatusCode(500, new
            {
                message = "Exception tracked successfully",
                exceptionType = ex.GetType().Name,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Demonstrates slow operation tracking
    /// </summary>
    [HttpGet("slow-operation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TrackSlowOperation()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Simulate slow operation
        var delay = Random.Shared.Next(1000, 3000);
        await Task.Delay(delay);

        stopwatch.Stop();

        // Track slow operation using custom telemetry service
        _telemetryService.TrackSlowOperation(
            "DemoSlowOperation", 
            stopwatch.ElapsedMilliseconds,
            new Dictionary<string, string>
            {
                { "ExpectedDelay", delay.ToString() },
                { "ActualDelay", stopwatch.ElapsedMilliseconds.ToString() }
            });

        return Ok(new
        {
            message = "Slow operation completed and tracked",
            durationMs = stopwatch.ElapsedMilliseconds,
            expectedDelayMs = delay,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Demonstrates business metric tracking
    /// </summary>
    [HttpGet("business-metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult TrackBusinessMetrics()
    {
        // Simulate business metrics
        var dailyActiveUsers = Random.Shared.Next(50, 150);
        var revenue = Random.Shared.Next(1000, 5000);
        var conversionRate = Random.Shared.NextDouble() * 0.1; // 0-10%

        _telemetryService.TrackBusinessMetric("DailyActiveUsers", dailyActiveUsers);
        _telemetryService.TrackBusinessMetric("DailyRevenue", revenue, new Dictionary<string, string>
        {
            { "Currency", "USD" },
            { "Date", DateTime.UtcNow.ToString("yyyy-MM-dd") }
        });
        _telemetryService.TrackBusinessMetric("ConversionRate", conversionRate);

        return Ok(new
        {
            message = "Business metrics tracked successfully",
            metrics = new
            {
                dailyActiveUsers,
                dailyRevenue = revenue,
                conversionRatePercent = Math.Round(conversionRate * 100, 2)
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get telemetry statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetTelemetryStats()
    {
        return Ok(new
        {
            message = "Application Insights telemetry is active",
            features = new[]
            {
                "Automatic request tracking",
                "Exception tracking", 
                "Dependency tracking",
                "Custom events",
                "Custom metrics",
                "Performance counters"
            },
            endpoints = new
            {
                customEvent = "/api/telemetrydemo/custom-event",
                customMetric = "/api/telemetrydemo/custom-metric",
                dependencyTracking = "/api/telemetrydemo/dependency-tracking",
                trackException = "/api/telemetrydemo/track-exception",
                slowOperation = "/api/telemetrydemo/slow-operation",
                businessMetrics = "/api/telemetrydemo/business-metrics"
            },
            timestamp = DateTime.UtcNow
        });
    }
}