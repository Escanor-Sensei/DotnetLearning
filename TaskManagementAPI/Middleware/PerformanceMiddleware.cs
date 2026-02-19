using System.Diagnostics;

namespace TaskManagementAPI.Middleware;

public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;

    public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";

        try
        {
            // Process request
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Add performance header to response
            context.Response.Headers["X-Processing-Time"] = $"{elapsedMs}ms";

            // Log performance metrics
            _logger.LogInformation("Request {CorrelationId} completed in {ElapsedMs}ms - {Method} {Path} -> {StatusCode}",
                correlationId, elapsedMs, context.Request.Method, context.Request.Path, context.Response.StatusCode);

            // Log warning for slow requests (over 2 seconds)
            if (elapsedMs > 2000)
            {
                _logger.LogWarning("Slow request detected {CorrelationId}: {Method} {Path} took {ElapsedMs}ms",
                    correlationId, context.Request.Method, context.Request.Path, elapsedMs);
            }

            // Log critical for very slow requests (over 5 seconds)
            if (elapsedMs > 5000)
            {
                _logger.LogCritical("Critical performance issue {CorrelationId}: {Method} {Path} took {ElapsedMs}ms",
                    correlationId, context.Request.Method, context.Request.Path, elapsedMs);
            }
        }
    }
}