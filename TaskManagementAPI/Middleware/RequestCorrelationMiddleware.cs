namespace TaskManagementAPI.Middleware;

public class RequestCorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestCorrelationMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public RequestCorrelationMiddleware(RequestDelegate next, ILogger<RequestCorrelationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or get existing correlation ID
        var correlationId = GetOrGenerateCorrelationId(context);
        
        // Store in HttpContext for other middleware and controllers
        context.Items["CorrelationId"] = correlationId;
        
        // Add to response headers for client tracking
        if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
        }

        // Create logging scope with correlation ID
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path.Value ?? "unknown",
            ["RequestMethod"] = context.Request.Method,
            ["UserAgent"] = context.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown",
            ["RemoteIP"] = GetClientIpAddress(context)
        });

        _logger.LogInformation("Request started");

        try
        {
            await _next(context);
            
            _logger.LogInformation("Request completed successfully with status {StatusCode}", 
                context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request failed with unhandled exception");
            throw; // Re-throw to be handled by GlobalExceptionHandlingMiddleware
        }
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Check if correlation ID exists in request headers
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();
        
        // If not provided, generate a new one
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N")[..8]; // Short 8-character ID
        }

        return correlationId;
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP (when behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for real IP (another common header)
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}