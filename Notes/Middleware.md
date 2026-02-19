# .NET Middleware - Creation & Testing Guide

## 🔧 **What is Middleware?**

Middleware is software that acts as a bridge between different applications or components in a system. In ASP.NET Core, middleware components form a **request pipeline** that handles HTTP requests and responses.

## 🏗️ **Middleware Pipeline Architecture**

```
Request → [Middleware 1] → [Middleware 2] → [Middleware 3] → Controller
Response ← [Middleware 1] ← [Middleware 2] ← [Middleware 3] ← Controller
```

### **Key Characteristics**
- **Sequential Processing**: Each middleware can process the request before passing it to the next
- **Bidirectional**: Can modify both incoming requests and outgoing responses
- **Short-circuiting**: Middleware can terminate the pipeline early
- **Order Matters**: The sequence of middleware registration affects execution

## 📝 **Creating Custom Middleware**

### **Method 1: Inline Middleware**
```csharp
app.Use((context, next) =>
{
    // Before next middleware
    Console.WriteLine($"Request: {context.Request.Path}");
    
    // Call next middleware
    var task = next();
    
    // After next middleware (on response)
    task.ContinueWith(_ => 
    {
        Console.WriteLine($"Response: {context.Response.StatusCode}");
    });
    
    return task;
});
```

### **Method 2: Custom Middleware Class**
```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Before processing request
        _logger.LogInformation("Processing request: {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        // Call next middleware
        await _next(context);

        // After processing request
        _logger.LogInformation("Completed request: {StatusCode}", 
            context.Response.StatusCode);
    }
}
```

### **Method 3: Extension Method Approach**
```csharp
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}

// Usage in Program.cs
app.UseRequestLogging();
```

## 🎯 **Common Middleware Use Cases**

### **1. Request Timing Middleware**
- Measures request processing time
- Adds performance headers
- Logs slow requests

### **2. Error Handling Middleware** 
- Catches unhandled exceptions
- Returns consistent error responses
- Logs error details

### **3. Security Middleware**
- Adds security headers
- Request validation
- Rate limiting

### **4. Logging Middleware**
- Request/response logging
- Correlation ID tracking
- Audit trails

### **5. Response Modification Middleware**
- Add custom headers
- Response caching
- Content transformation

## 🧪 **Testing Middleware**

### **Unit Testing Approach**
```csharp
[Test]
public async Task RequestLoggingMiddleware_LogsRequestAndResponse()
{
    // Arrange
    var logger = new Mock<ILogger<RequestLoggingMiddleware>>();
    var context = new DefaultHttpContext();
    context.Request.Method = "GET";
    context.Request.Path = "/test";
    
    var middleware = new RequestLoggingMiddleware(
        (ctx) => { ctx.Response.StatusCode = 200; return Task.CompletedTask; },
        logger.Object
    );

    // Act
    await middleware.InvokeAsync(context);

    // Assert
    Assert.AreEqual(200, context.Response.StatusCode);
    // Verify logger was called with expected parameters
}
```

### **Integration Testing**
```csharp
[Test]
public async Task Api_WithCustomMiddleware_ReturnsExpectedHeaders()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/tasks");

    // Assert
    Assert.IsTrue(response.Headers.Contains("X-Processing-Time"));
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
}
```

## 🔧 **Practical Implementation Examples**

### **Example 1: Performance Monitoring**
```csharp
public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds;
            
            // Add header
            context.Response.Headers.Add("X-Processing-Time", $"{elapsed}ms");
            
            // Log slow requests
            if (elapsed > 1000)
            {
                _logger.LogWarning("Slow request detected: {Method} {Path} took {ElapsedMs}ms",
                    context.Request.Method, context.Request.Path, elapsed);
            }
        }
    }
}
```

### **Example 2: Global Error Handling**
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            NotFoundException => new { error = "Resource not found", status = 404 },
            ValidationException => new { error = "Validation failed", status = 400 },
            UnauthorizedAccessException => new { error = "Unauthorized", status = 401 },
            _ => new { error = "Internal server error", status = 500 }
        };

        context.Response.StatusCode = response.status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

### **Example 3: Request ID Middleware**
```csharp
public class RequestIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string RequestIdHeader = "X-Request-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or get existing request ID
        var requestId = context.Request.Headers[RequestIdHeader].FirstOrDefault() 
                       ?? Guid.NewGuid().ToString();

        // Add to context for logging
        context.Items["RequestId"] = requestId;
        
        // Add to response headers
        context.Response.Headers.Add(RequestIdHeader, requestId);

        await _next(context);
    }
}
```

## 🎛️ **Middleware Registration Order**

**Critical**: Order matters! Here's the recommended sequence:

```csharp
// Program.cs - Correct order
app.UseExceptionHandler();     // 1. Exception handling (first)
app.UseHttpsRedirection();     // 2. HTTPS redirection
app.UseRouting();              // 3. Routing
app.UseAuthentication();       // 4. Authentication (who you are)
app.UseAuthorization();        // 5. Authorization (what you can do)
app.UseCustomMiddleware();     // 6. Custom middleware
app.MapControllers();          // 7. Endpoint mapping (last)
```

## ✅ **Testing Strategies**

### **1. Unit Testing Individual Middleware**
- Test middleware logic in isolation
- Mock dependencies (logger, services)
- Test different scenarios (success, failure, edge cases)

### **2. Integration Testing**
- Test middleware within the full pipeline
- Verify middleware interaction
- Test with real HTTP requests

### **3. Performance Testing**
- Measure middleware overhead
- Test under load
- Monitor memory usage

## 📊 **Best Practices**

### **DO's ✅**
- Keep middleware lightweight and focused
- Handle exceptions gracefully
- Use dependency injection properly
- Add comprehensive logging
- Test thoroughly
- Document middleware purpose

### **DON'Ts ❌**
- Don't perform heavy computations
- Don't ignore the next delegate
- Don't modify response after next() completes
- Don't forget to handle exceptions
- Don't create tight coupling

## 🔍 **Debugging Middleware**

### **Logging Strategy**
```csharp
public async Task InvokeAsync(HttpContext context)
{
    var correlationId = Guid.NewGuid().ToString()[..8];
    
    using var scope = _logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = correlationId,
        ["RequestPath"] = context.Request.Path
    });

    _logger.LogInformation("Middleware started");
    
    try
    {
        await _next(context);
        _logger.LogInformation("Middleware completed successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Middleware failed");
        throw;
    }
}
```

## 🎓 **Learning Benefits**

1. **Request Pipeline Understanding** - How ASP.NET Core processes requests
2. **Cross-Cutting Concerns** - Implementing features that span multiple layers
3. **Performance Optimization** - Monitoring and improving request processing
4. **Error Handling** - Consistent error responses across the application
5. **Testing Skills** - Unit and integration testing of middleware components

---

*Middleware is the backbone of ASP.NET Core's request processing. Understanding how to create, test, and optimize middleware is crucial for building robust web applications.*