namespace TaskManagementAPI.Middleware;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds request correlation middleware to track requests with unique IDs
    /// </summary>
    public static IApplicationBuilder UseRequestCorrelation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestCorrelationMiddleware>();
    }

    /// <summary>
    /// Adds global exception handling middleware for consistent error responses
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }

    /// <summary>
    /// Adds performance monitoring middleware to track request processing times
    /// </summary>
    public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PerformanceMiddleware>();
    }

    /// <summary>
    /// Adds rate limiting middleware with specified options
    /// </summary>
    /// <param name="builder">Application builder</param>
    /// <param name="configureOptions">Rate limiting configuration</param>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder, 
        Action<RateLimitOptions>? configureOptions = null)
    {
        var options = new RateLimitOptions();
        configureOptions?.Invoke(options);
        
        return builder.UseMiddleware<RateLimitingMiddleware>(options);
    }

    /// <summary>
    /// Adds all custom middlewares in the correct order
    /// </summary>
    /// <param name="builder">Application builder</param>
    /// <param name="rateLimitOptions">Optional rate limiting configuration</param>
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder,
        Action<RateLimitOptions>? rateLimitOptions = null)
    {
        // Order is important!
        return builder
            .UseRequestCorrelation()           // 1. Add correlation ID first
            .UseGlobalExceptionHandling()      // 2. Exception handling early
            .UseRateLimiting(rateLimitOptions) // 3. Rate limiting before auth
            .UsePerformanceMonitoring();       // 4. Performance monitoring throughout
    }
}