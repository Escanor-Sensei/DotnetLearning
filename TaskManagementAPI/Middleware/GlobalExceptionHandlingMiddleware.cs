using System.Net;
using System.Text.Json;

namespace TaskManagementAPI.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            _logger.LogError(ex, "An unhandled exception occurred for request {CorrelationId} - {Method} {Path}",
                correlationId, context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex, correlationId);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
    {
        context.Response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            ArgumentNullException => CreateErrorResponse(
                "Invalid input: Required parameter is null", 
                HttpStatusCode.BadRequest,
                correlationId),

            ArgumentException => CreateErrorResponse(
                "Invalid input: " + exception.Message, 
                HttpStatusCode.BadRequest,
                correlationId),

            UnauthorizedAccessException => CreateErrorResponse(
                "Access denied: You don't have permission to perform this action", 
                HttpStatusCode.Forbidden,
                correlationId),

            KeyNotFoundException => CreateErrorResponse(
                "Resource not found: The requested item does not exist", 
                HttpStatusCode.NotFound,
                correlationId),

            InvalidOperationException => CreateErrorResponse(
                "Operation failed: " + exception.Message, 
                HttpStatusCode.Conflict,
                correlationId),

            TimeoutException => CreateErrorResponse(
                "Request timeout: The operation took too long to complete", 
                HttpStatusCode.RequestTimeout,
                correlationId),

            _ => CreateErrorResponse(
                "An internal server error occurred. Please try again later.", 
                HttpStatusCode.InternalServerError,
                correlationId)
        };

        context.Response.StatusCode = (int)errorResponse.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(new
        {
            error = new
            {
                message = errorResponse.Message,
                correlationId = errorResponse.CorrelationId,
                timestamp = errorResponse.Timestamp,
                statusCode = (int)errorResponse.StatusCode
            }
        }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static ErrorResponse CreateErrorResponse(string message, HttpStatusCode statusCode, string correlationId)
    {
        return new ErrorResponse
        {
            Message = message,
            StatusCode = statusCode,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public HttpStatusCode StatusCode { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}