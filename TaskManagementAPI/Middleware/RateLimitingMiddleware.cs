using System.Collections.Concurrent;

namespace TaskManagementAPI.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;
    
    // In-memory storage for rate limiting (in production, use Redis or similar)
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> ClientRequests = new();

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, RateLimitOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for certain paths
        if (ShouldSkipRateLimit(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        var clientInfo = ClientRequests.AddOrUpdate(clientId, 
            new ClientRequestInfo { RequestCount = 1, WindowStart = now }, 
            (key, existing) => UpdateClientInfo(existing, now));

        // Check if client has exceeded rate limit
        if (clientInfo.RequestCount > _options.RequestLimit)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            _logger.LogWarning("Rate limit exceeded for client {ClientId} with correlation {CorrelationId}. " +
                              "Requests: {RequestCount}/{RequestLimit} in {WindowMinutes} minutes",
                clientId, correlationId, clientInfo.RequestCount, _options.RequestLimit, _options.WindowMinutes);

            // Return rate limit exceeded response
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.ContentType = "application/json";
            
            // Add rate limit headers
            var resetTime = clientInfo.WindowStart.AddMinutes(_options.WindowMinutes);
            context.Response.Headers["X-RateLimit-Limit"] = _options.RequestLimit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(resetTime).ToUnixTimeSeconds().ToString();
            context.Response.Headers["Retry-After"] = ((int)(resetTime - now).TotalSeconds).ToString();

            var errorResponse = new
            {
                error = new
                {
                    message = $"Rate limit exceeded. Maximum {_options.RequestLimit} requests per {_options.WindowMinutes} minutes.",
                    retryAfter = (int)(resetTime - now).TotalSeconds,
                    correlationId = correlationId
                }
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
            return;
        }

        // Add rate limit headers for successful requests
        var remaining = Math.Max(0, _options.RequestLimit - clientInfo.RequestCount);
        context.Response.Headers["X-RateLimit-Limit"] = _options.RequestLimit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        
        var nextReset = clientInfo.WindowStart.AddMinutes(_options.WindowMinutes);
        context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(nextReset).ToUnixTimeSeconds().ToString();

        // Continue to next middleware
        await _next(context);
    }

    private static bool ShouldSkipRateLimit(PathString path)
    {
        var pathsToSkip = new[] { "/health", "/swagger", "/api/auth/test-users" };
        return pathsToSkip.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Priority: User ID > IP Address
        var userId = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private ClientRequestInfo UpdateClientInfo(ClientRequestInfo existing, DateTime now)
    {
        var windowExpired = now > existing.WindowStart.AddMinutes(_options.WindowMinutes);
        
        if (windowExpired)
        {
            // Reset window
            return new ClientRequestInfo 
            { 
                RequestCount = 1, 
                WindowStart = now 
            };
        }

        // Increment count within current window
        return new ClientRequestInfo 
        { 
            RequestCount = existing.RequestCount + 1, 
            WindowStart = existing.WindowStart 
        };
    }

    // Clean up old entries periodically (basic implementation)
    static RateLimitingMiddleware()
    {
        // Run cleanup every 5 minutes
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(5));
                CleanupOldEntries();
            }
        });
    }

    private static void CleanupOldEntries()
    {
        var cutoff = DateTime.UtcNow.AddHours(-1); // Remove entries older than 1 hour
        var keysToRemove = ClientRequests
            .Where(kvp => kvp.Value.WindowStart < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            ClientRequests.TryRemove(key, out _);
        }
    }
}

public class ClientRequestInfo
{
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
}

public class RateLimitOptions
{
    public int RequestLimit { get; set; } = 100; // Max requests
    public int WindowMinutes { get; set; } = 15; // Time window in minutes
}