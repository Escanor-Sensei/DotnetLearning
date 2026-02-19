# .NET Middleware

## What is Middleware?

Middleware is software assembled into an application pipeline to handle requests and responses. Each component in the pipeline:
- **Chooses** whether to pass the request to the next component
- **Can perform work** before and after the next component executes

In ASP.NET Core, middleware forms the backbone of the HTTP request processing pipeline. Every request passes through a chain of middleware components, and each can inspect, modify, short-circuit, or pass along the request.

## The Request Pipeline

### How It Works

```
Request  →  [M1: before]  →  [M2: before]  →  [M3: before]  →  Endpoint
Response ←  [M1: after]   ←  [M2: after]   ←  [M3: after]   ←  Endpoint
```

Key characteristics:
- **Bidirectional** — Each middleware runs code _before_ calling `next()` (request phase) and _after_ `next()` returns (response phase)
- **Sequential** — Registration order determines execution order
- **Short-circuiting** — A middleware can return a response without calling `next()`, terminating the pipeline early
- **Composable** — Middleware components are independent and reusable

### The `next` Delegate

The `RequestDelegate next` parameter represents the next middleware in the pipeline. Calling `await next(context)` passes control to the next middleware. Not calling it short-circuits the pipeline.

```
If next() is called:    Request flows deeper into pipeline, then response flows back
If next() is NOT called: Pipeline stops here, response is returned immediately
```

## Creating Custom Middleware

There are three approaches to creating middleware, each suited for different scenarios:

### Approach 1: Inline Middleware (Lambda)

- Defined directly in `Program.cs` using `app.Use()` or `app.Run()`
- Best for: Simple, one-off middleware logic
- `app.Use()` can call `next()` to continue the pipeline
- `app.Run()` is terminal — it never calls `next()`

### Approach 2: Convention-Based Middleware Class

The most common approach. The class must have:
- A constructor accepting `RequestDelegate next` (and any DI services with **singleton** lifetime)
- A public method `InvokeAsync(HttpContext context)` or `Invoke(HttpContext context)`
- Scoped/transient services are injected as parameters on `InvokeAsync`, not in the constructor

**Why?** Middleware is instantiated once at startup (singleton lifetime). Injecting scoped services in the constructor would cause them to live for the entire application lifetime, leading to bugs.

### Approach 3: Factory-Based Middleware (IMiddleware)

- Implement the `IMiddleware` interface
- Registered in DI container, allowing any service lifetime (scoped, transient)
- More testable due to explicit interface
- Must be registered: `builder.Services.AddTransient<MyMiddleware>()`

### Extension Method Pattern

Best practice is to create an extension method for registering middleware:

```csharp
public static IApplicationBuilder UseMyMiddleware(this IApplicationBuilder builder)
    => builder.UseMiddleware<MyMiddleware>();
```

This keeps `Program.cs` clean and creates a consistent registration API.

## Middleware Registration Order

**Order is critical.** The sequence you register middleware in `Program.cs` determines execution order. The standard recommended order is:

| Order | Middleware | Purpose |
|---|---|---|
| 1 | Exception Handler | Catches all unhandled exceptions (must be first to catch everything) |
| 2 | HSTS | Strict transport security header |
| 3 | HTTPS Redirection | Redirects HTTP to HTTPS |
| 4 | Static Files | Serves static files (short-circuits if file found) |
| 5 | Routing | Determines which endpoint matches the request |
| 6 | CORS | Cross-origin resource sharing |
| 7 | Authentication | Establishes identity (who you are) |
| 8 | Authorization | Checks permissions (what you can do) |
| 9 | Custom Middleware | Your application-specific middleware |
| 10 | Endpoint Mapping | Executes the matched endpoint (controller action) |

### Why Order Matters — Examples

- **Exception handling first**: If placed after authentication, auth failures won't get proper error formatting
- **Authentication before authorization**: Authorization needs to know who the user is
- **Routing before authorization**: Authorization policies often depend on the matched route/endpoint metadata
- **CORS before auth**: Preflight OPTIONS requests need CORS headers before auth rejects them

## Common Middleware Patterns

### 1. Request/Response Logging

Captures request details (method, path, headers) before processing and response details (status code, duration) after. Useful for auditing and debugging.

**Key consideration**: Buffer the response body stream if you need to read it, since the response stream is forward-only by default.

### 2. Global Exception Handling

Wraps the entire pipeline in a try-catch. When an exception occurs:
- Logs the full exception details
- Returns a consistent error response format (e.g., ProblemDetails)
- Prevents stack traces from leaking to clients in production
- Can return different responses based on exception type (NotFound → 404, Validation → 400, etc.)

### 3. Performance/Timing Middleware

Starts a `Stopwatch` before `next()` and stops it after. Can:
- Add timing headers (e.g., `X-Processing-Time`)
- Log slow requests that exceed a threshold
- Feed metrics to monitoring systems

### 4. Correlation ID / Request ID

Generates or propagates a unique identifier for each request:
- Checks if `X-Correlation-ID` header exists on the incoming request
- If not, generates a new GUID
- Adds it to response headers and makes it available for logging
- Enables tracing a single request across multiple services and log entries

### 5. Security Headers

Adds defensive HTTP headers to responses:
- `X-Content-Type-Options: nosniff` — Prevents MIME type sniffing
- `X-Frame-Options: DENY` — Prevents clickjacking
- `X-XSS-Protection` — Browser XSS filter
- `Content-Security-Policy` — Controls resource loading
- `Strict-Transport-Security` — Forces HTTPS

### 6. Rate Limiting

Tracks requests per client (by IP, API key, or user) within a time window. Returns `429 Too Many Requests` when the limit is exceeded. Approaches:
- **Fixed window** — Simple counter per time window
- **Sliding window** — More accurate, avoids burst at window boundaries
- **Token bucket** — Allows burst traffic up to a limit
- **ASP.NET Core built-in** — `AddRateLimiter()` (available in .NET 7+)

## Middleware vs Action Filters

| Aspect | Middleware | Action Filters |
|---|---|---|
| **Scope** | All requests (including static files, non-MVC) | Only MVC/API controller actions |
| **Access to** | HttpContext only | ActionContext, ModelState, action arguments |
| **Pipeline position** | Earlier in pipeline, wraps everything | Inside MVC pipeline, closer to action |
| **Use for** | Cross-cutting: logging, CORS, auth, error handling | MVC-specific: validation, caching, authorization policies |
| **Short-circuit** | Returns response directly | Sets `context.Result` |

**Rule of thumb**: If it applies to _all_ HTTP requests, use middleware. If it applies only to _controller actions_, use filters.

## Dependency Injection in Middleware

### Singleton Services (Constructor Injection)

Services with singleton lifetime can be injected via the constructor because middleware itself is singleton.

### Scoped/Transient Services (Method Injection)

Scoped or transient services must be injected as parameters on `InvokeAsync()`, not in the constructor. This ensures a new instance is created per request.

Common pattern for accessing scoped services:

```
InvokeAsync(HttpContext context, IMyService service, ILogger<MyMiddleware> logger)
```

The DI container resolves these parameters fresh for each request.

## Middleware Best Practices

### Do
- Keep middleware **focused** — each middleware should do one thing
- Handle **exceptions** in middleware to prevent pipeline crashes
- Use **async/await** properly — never block with `.Result` or `.Wait()`
- Make middleware **configurable** through options pattern
- Add **logging** for debugging and monitoring
- Ensure proper **disposal** of resources
- Test middleware in **isolation** with unit tests

### Don't
- Perform **heavy computation** — middleware runs on every request
- Modify the **response body after calling next()** — headers may already be sent
- Forget to call `next()` unless intentionally short-circuiting
- Create **tight coupling** between middleware components
- Store **per-request state** in middleware fields (middleware is singleton!)
- Throw exceptions for **expected conditions** — use proper response codes instead

## Terminal vs Non-Terminal Middleware

| Type | Behavior | Registration |
|---|---|---|
| **Non-terminal** | Calls `next()`, request continues through pipeline | `app.Use()` |
| **Terminal** | Does NOT call `next()`, pipeline stops here | `app.Run()` |
| **Conditional** | Only runs for specific paths/conditions | `app.Map()`, `app.MapWhen()` |

### Branch Middleware

- `app.Map("/path", branch => ...)` — Branches the pipeline for a specific path prefix
- `app.MapWhen(predicate, branch => ...)` — Branches based on any condition
- `app.UseWhen(predicate, branch => ...)` — Like `MapWhen` but re-joins the main pipeline

## Advanced Concepts

### Middleware Pipeline Branching

The pipeline can split into branches that handle requests independently. Useful for:
- Different processing for API vs MVC requests
- Health check endpoints that bypass most middleware
- WebSocket upgrade handling

### Middleware vs Endpoint Routing

With endpoint routing (introduced in .NET Core 3.0):
- **Routing middleware** (`UseRouting`) determines which endpoint matches the request
- **Endpoint middleware** (`UseEndpoints`/`MapControllers`) executes the matched endpoint
- Middleware between these two can access endpoint metadata (e.g., `[Authorize]` attributes)

This separation allows authorization middleware to see endpoint-specific policies before the action executes.

### HttpContext.Items

A per-request dictionary for passing data between middleware components. Any middleware can write to `context.Items["key"]` and subsequent middleware can read it. Useful for sharing computed values (e.g., correlation IDs, timing data) without coupling middleware classes.

---

*Middleware is the backbone of ASP.NET Core's request processing. Understanding the pipeline, execution order, and proper patterns is essential for building robust, maintainable web applications.*