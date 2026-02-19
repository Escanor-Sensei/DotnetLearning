# Middleware Testing

## Why Test Middleware?

Middleware sits at the foundation of the HTTP pipeline — bugs here affect _every_ request. Testing middleware ensures:
- Cross-cutting logic (logging, error handling, security headers) works correctly
- Pipeline short-circuiting behaves as expected
- Request/response modifications are applied properly
- Performance overhead is acceptable

## Testing Approaches

### 1. Unit Testing (Isolated)

Tests middleware logic in isolation by providing a fake `HttpContext` and a mock `RequestDelegate`.

**How it works:**
- Create a `DefaultHttpContext` — a lightweight, in-memory HTTP context
- Create a mock/stub `RequestDelegate` (the "next" middleware)
- Instantiate the middleware with the mock delegate and mocked dependencies
- Call `InvokeAsync(context)` directly
- Assert on the context's response (headers, status code, body)

**What you're testing:**
- Individual middleware behavior
- How it modifies requests and responses
- Error handling within the middleware
- Whether it calls or skips `next()`

**Key patterns:**
| Pattern | Purpose |
|---|---|
| Mock `RequestDelegate` | Control what the "next" middleware does |
| `DefaultHttpContext` | Simulate HTTP request/response without a real server |
| Mock `ILogger` | Verify logging behavior |
| Assert response headers | Verify middleware added expected headers |
| Assert `next()` was called | Verify middleware didn't short-circuit unexpectedly |

### 2. Integration Testing (Full Pipeline)

Tests middleware within the complete ASP.NET Core pipeline using `WebApplicationFactory<Program>`.

**How it works:**
- `WebApplicationFactory` boots the entire application in-memory
- You get an `HttpClient` that sends real HTTP requests through the full pipeline
- All middleware, routing, DI, and configuration run as in production
- You can override services/configuration for test-specific behavior

**What you're testing:**
- Middleware interaction with other middleware
- Correct middleware registration order
- Headers present in actual HTTP responses
- End-to-end request processing

**Key concepts:**

| Concept | Description |
|---|---|
| `WebApplicationFactory<Program>` | Creates an in-memory test server with full pipeline |
| `IClassFixture<>` | Shares a single factory instance across all tests in a class |
| `ConfigureWebHost()` | Override services, configuration, or middleware for tests |
| `CreateClient()` | Returns an HttpClient pre-configured to call the test server |

### 3. Performance Testing

Validates that middleware doesn't introduce unacceptable overhead.

**What to test:**
- Processing time added by middleware is within acceptable limits
- Middleware behaves correctly under concurrent requests
- Memory allocations per request are reasonable
- No resource leaks over time

## Testing Specific Middleware Types

### Performance/Timing Middleware
- Verify `X-Processing-Time` header (or equivalent) is present in responses
- Verify the header value is a valid duration format
- Verify slow request thresholds trigger appropriate logging
- Mock a slow `next()` delegate to test the slow-request detection path

### Exception Handling Middleware
- Make `next()` throw different exception types
- Verify each exception type maps to the correct HTTP status code (404, 400, 500, etc.)
- Verify the error response body has a consistent format (e.g., always includes `message`, `correlationId`)
- Verify the original exception details are NOT leaked to the client in production mode
- Verify exceptions are logged with appropriate severity

### Correlation ID Middleware
- Verify a new correlation ID is generated when none is provided
- Verify an existing correlation ID from the request header is preserved (passed through)
- Verify the correlation ID appears in the response headers
- Verify the ID is available in `HttpContext.Items` for downstream middleware

### Rate Limiting Middleware
- Send N requests within the allowed window — all should succeed (200)
- Send N+1 requests — should receive `429 Too Many Requests`
- Wait for the window to reset — requests should succeed again
- Test with different client identifiers to ensure limits are per-client

### Security Headers Middleware
- Verify each security header is present in responses
- Verify header values match expected security policies
- Verify headers are present on both success and error responses

## Arrange-Act-Assert for Middleware

The standard test structure for middleware:

```
ARRANGE:
  - Create DefaultHttpContext (set Method, Path, Headers as needed)
  - Create mock RequestDelegate (define what "next" does)
  - Mock dependencies (ILogger, services)
  - Instantiate middleware

ACT:
  - Call middleware.InvokeAsync(context)

ASSERT:
  - Check response status code
  - Check response headers
  - Check response body (if applicable)
  - Verify next() was called (or not)
  - Verify logger was called with expected messages
```

## Testing Challenges & Solutions

| Challenge | Solution |
|---|---|
| **Response body is a non-readable stream** | Replace `context.Response.Body` with a `MemoryStream`, then read it after `InvokeAsync` |
| **Middleware needs scoped services** | Use `WebApplicationFactory` integration tests, or manually create a service scope in unit tests |
| **Timing tests are flaky** | Assert on _relative_ timing (duration > 0) rather than exact values; use reasonable thresholds |
| **Middleware order matters** | Integration tests validate order implicitly; for unit tests, test each middleware independently |
| **Async exception handling** | Ensure `await` is used properly; use `Assert.ThrowsAsync<>` for exception assertions |

## Test Naming Conventions

Good middleware test names follow the pattern:

```
[MiddlewareName]_[Scenario]_[ExpectedBehavior]

Examples:
- PerformanceMiddleware_NormalRequest_AddsProcessingTimeHeader
- ExceptionMiddleware_WhenNotFoundThrown_Returns404
- CorrelationMiddleware_WhenHeaderProvided_PreservesExistingId
- RateLimitMiddleware_WhenLimitExceeded_Returns429
```

## Coverage Goals

| Test Type | Coverage Target | Focus Area |
|---|---|---|
| **Unit Tests** | 90%+ of middleware code paths | Logic, branching, error handling |
| **Integration Tests** | All middleware endpoints | Pipeline interaction, header propagation |
| **Performance Tests** | Critical paths | Timing overhead, concurrency behavior |

## Common Anti-Patterns in Middleware Testing

| Anti-Pattern | Why It's Bad | Better Approach |
|---|---|---|
| Testing the ASP.NET framework itself | Wastes time, framework is already tested | Focus on YOUR middleware logic |
| Exact timing assertions | Flaky on different machines/CI | Use relative/threshold assertions |
| Not cleaning up test resources | Memory leaks, port conflicts | Implement `IDisposable`/`IAsyncDisposable` |
| Testing multiple behaviors in one test | Hard to diagnose failures | One assertion concern per test |
| Ignoring the response phase | Misses bugs in post-`next()` code | Assert on both request and response modifications |

---

*Testing middleware thoroughly ensures that your cross-cutting concerns — the invisible backbone of your application — work reliably under all conditions.*