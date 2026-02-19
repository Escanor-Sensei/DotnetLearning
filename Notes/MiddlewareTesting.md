# Middleware Testing Guide

This document demonstrates how to test the custom middleware components in the TaskManagementAPI project.

## 🧪 **Testing Overview**

The project includes several types of middleware testing:

1. **Unit Tests** - Test middleware logic in isolation
2. **Integration Tests** - Test middleware within the full HTTP pipeline
3. **Performance Tests** - Validate timing and rate limiting

## 🔧 **Test Setup**

### **Required NuGet Packages**
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.TestHost" Version="8.0.0" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="xunit" Version="2.4.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
```

### **Creating Test Project**
```bash
# Navigate to project root
cd "Dotnet Learning"

# Create test project
dotnet new xunit -n TaskManagementAPI.Tests

# Add project reference
cd TaskManagementAPI.Tests
dotnet add reference ../TaskManagementAPI/TaskManagementAPI.csproj

# Add test packages
dotnet add package Moq
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.AspNetCore.TestHost
```

## 🧪 **Unit Testing Middleware**

Unit tests verify middleware behavior in isolation using mocked dependencies.

### **Example: Testing Performance Middleware**
```csharp
[Fact]
public async Task PerformanceMiddleware_ShouldAddProcessingTimeHeader()
{
    // Arrange
    var loggerMock = new Mock<ILogger<PerformanceMiddleware>>();
    var nextMock = new Mock<RequestDelegate>();
    var context = new DefaultHttpContext();
    
    nextMock.Setup(next => next(It.IsAny<HttpContext>()))
           .Returns(Task.CompletedTask);

    var middleware = new PerformanceMiddleware(nextMock.Object, loggerMock.Object);

    // Act
    await middleware.InvokeAsync(context);

    // Assert
    Assert.True(context.Response.Headers.ContainsKey("X-Processing-Time"));
    nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Once);
}
```

### **Key Unit Testing Patterns**
- **Mock RequestDelegate** to control next middleware
- **Use DefaultHttpContext** for HTTP context simulation
- **Verify headers and status codes** in response
- **Test exception scenarios** separately

## 🌐 **Integration Testing Middleware**

Integration tests validate middleware within the complete HTTP pipeline.

### **WebApplicationFactory Setup**
```csharp
public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> 
    where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Override services for testing
            services.AddSingleton<ILogger<PerformanceMiddleware>, TestLogger>();
        });
    }
}
```

### **Integration Test Examples**
```csharp
public class MiddlewareIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public MiddlewareIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ShouldIncludeCorrelationIdAndProcessingTime()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        Assert.True(response.Headers.Contains("X-Processing-Time"));
    }

    [Fact]
    public async Task GetHealthError_ShouldReturnConsistentErrorFormat()
    {
        // Act
        var response = await _client.GetAsync("/api/health/error");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content);
        
        Assert.NotNull(errorResponse.CorrelationId);
        Assert.Contains("test exception", errorResponse.Message.ToLower());
    }
}
```

## ⚡ **Performance Testing**

### **Testing Request Timing**
```csharp
[Fact]
public async Task SlowEndpoint_ShouldLogWarningForSlowRequests()
{
    // Arrange
    var loggerMock = new Mock<ILogger<PerformanceMiddleware>>();
    
    // Act
    var response = await _client.GetAsync("/api/health/slow");
    
    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
    var processingTime = response.Headers.GetValues("X-Processing-Time").FirstOrDefault();
    Assert.NotNull(processingTime);
    
    // Verify time is tracked
    var timeMs = int.Parse(processingTime.Replace("ms", ""));
    Assert.True(timeMs >= 3000); // Should be at least 3 seconds
}
```

### **Testing Rate Limiting**
```csharp
[Fact]
public async Task RateLimit_ShouldBlock_AfterExceedingLimit()
{
    // Arrange
    const int requestLimit = 5;
    var client = _factory.CreateClient();
    
    // Act - Make requests up to the limit
    var responses = new List<HttpResponseMessage>();
    for (int i = 0; i < requestLimit + 2; i++)
    {
        var response = await client.GetAsync("/api/health/load-test");
        responses.Add(response);
    }
    
    // Assert
    var successfulRequests = responses.Take(requestLimit).ToList();
    var blockedRequests = responses.Skip(requestLimit).ToList();
    
    // First 5 should succeed
    Assert.All(successfulRequests, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
    
    // Additional requests should be rate limited
    Assert.All(blockedRequests, r => Assert.Equal(HttpStatusCode.TooManyRequests, r.StatusCode));
}
```

## 📊 **Testing Correlation ID Tracking**

```csharp
[Fact]
public async Task CorrelationId_ShouldBeConsistent_AcrossMultipleRequests()
{
    // Arrange
    var correlationId = Guid.NewGuid().ToString("N")[..8];
    _client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
    
    // Act
    var response = await _client.GetAsync("/api/health");
    
    // Assert
    var responseCorrelationId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
    Assert.Equal(correlationId, responseCorrelationId);
}
```

## 🧪 **Testing Exception Handling**

```csharp
[Fact]
public async Task ExceptionMiddleware_ShouldReturnProperErrorFormat()
{
    // Act
    var response = await _client.GetAsync("/api/health/error");
    
    // Assert
    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    
    var content = await response.Content.ReadAsStringAsync();
    var errorObj = JsonDocument.Parse(content);
    
    Assert.True(errorObj.RootElement.TryGetProperty("error", out var errorProp));
    Assert.True(errorProp.TryGetProperty("correlationId", out _));
    Assert.True(errorProp.TryGetProperty("message", out _));
}
```

## 🚀 **Running Tests**

### **Command Line**
```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=PerformanceMiddlewareTests"

# Run with detailed output
dotnet test --verbosity normal

# Run with coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"
```

### **Visual Studio**
- Test Explorer → Run All Tests
- Right-click test → Run Test(s)
- Debug → Start Debugging (for test debugging)

## 📈 **Testing Best Practices**

### **DO's ✅**
- **Test both success and failure scenarios**
- **Verify middleware order in integration tests**
- **Mock external dependencies in unit tests**
- **Test middleware configuration options**
- **Validate all headers and status codes**
- **Use descriptive test names**

### **DON'Ts ❌**
- **Don't test framework functionality**
- **Don't create brittle timing tests**
- **Don't ignore test cleanup**
- **Don't test multiple concerns in one test**

## 🎯 **Test Coverage Goals**

- **Unit Tests**: 90%+ code coverage for middleware logic
- **Integration Tests**: All middleware endpoints tested end-to-end
- **Performance Tests**: Key scenarios validated for timing
- **Error Handling**: All exception types tested

## 📝 **Example Test Results**

```
Test run for TaskManagementAPI.Tests.dll (.NETCoreApp,Version=v8.0)
✓ PerformanceMiddleware_ShouldAddProcessingTimeHeader [23ms]
✓ PerformanceMiddleware_ShouldCallNextMiddleware [18ms]
✓ RequestCorrelation_ShouldGenerateCorrelationId [15ms]
✓ ExceptionHandling_ShouldReturnConsistentFormat [45ms]
✓ RateLimit_ShouldBlockAfterExceedingLimit [267ms]

Test Run Successful.
Total tests: 12
     Passed: 12
     Failed: 0
   Skipped: 0
 Total time: 2.3 seconds
```

---

*Testing middleware thoroughly ensures reliable operation and helps catch issues before they reach production. These examples provide a solid foundation for testing your custom middleware components.*