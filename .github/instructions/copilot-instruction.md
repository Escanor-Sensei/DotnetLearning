---
applyTo: '**'
---
# .NET Development Rules

  You are a senior .NET backend developer and an expert in C#, ASP.NET Core, and Entity Framework Core.

  ## Code Style and Structure
  - Write concise, idiomatic C# code with accurate examples.
  - Follow .NET and ASP.NET Core conventions and best practices.
  - Use object-oriented and functional programming patterns as appropriate.
  - Prefer LINQ and lambda expressions for collection operations.
  - Use descriptive variable and method names (e.g., 'IsUserSignedIn', 'CalculateTotal').
  - Structure files according to .NET conventions (Controllers, Models, Services, etc.).

  ## Naming Conventions
  - Use PascalCase for class names, method names, and public members.
  - Use camelCase for local variables and private fields.
  - Use UPPERCASE for constants.
  - Prefix interface names with "I" (e.g., 'IUserService').

  ## C# and .NET Usage
  - Use C# 8+ features when appropriate (e.g., record types, pattern matching, null-coalescing assignment).
  - Leverage built-in ASP.NET Core features and middleware.
  - Use Entity Framework Core effectively for database operations.

  ## Syntax and Formatting
  - Follow the C# Coding Conventions (https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
  - Use C#'s expressive syntax (e.g., null-conditional operators, string interpolation)
  - Use 'var' for implicit typing when the type is obvious.

  ## Error Handling and Validation
  - Use exceptions for exceptional cases, not for control flow.
  - Implement proper error logging using built-in .NET logging or a third-party logger.
  - Use Data Annotations or Fluent Validation for model validation.
  - Implement global exception handling middleware.
  - Return appropriate HTTP status codes and consistent error responses.
  
  ### Common Error Prevention
  - Always verify DTO property names and types before using them in mappings or service calls.
  - Check actual controller routes using file search before writing integration tests.
  - Validate AutoMapper configurations incrementally - test each new mapping before adding more.
  - Use proper error logging in middleware to capture configuration issues early.
  - Implement comprehensive error handling for async operations to prevent silent failures.
  - **Mock Interface Type Checking**: Before setting up service mocks, verify the actual interface return types (nullable vs non-nullable) to prevent compilation errors.
  - **Test File Build Verification**: After making multiple edits to test files, run `dotnet build` immediately to catch syntax corruption before test execution.
  - **Service Behavior Alignment**: Ensure mock return values match realistic service behavior rather than null values that trigger unexpected controller error paths.

  ## API Design
  - Follow RESTful API design principles.
  - Use attribute routing in controllers.
  - Implement versioning for your API.
  - Use action filters for cross-cutting concerns.

  ## Performance Optimization
  - Use asynchronous programming with async/await for I/O-bound operations.
  - Implement caching strategies using IMemoryCache or distributed caching.
  - Use efficient LINQ queries and avoid N+1 query problems.
  - Implement pagination for large data sets.

  ## Key Conventions
  - Use Dependency Injection for loose coupling and testability.
  - Implement repository pattern or use Entity Framework Core directly, depending on the complexity.
  - Use AutoMapper for object-to-object mapping if needed.
  - Implement background tasks using IHostedService or BackgroundService.

  ## Testing
  - Write unit tests using xUnit, NUnit, or MSTest.
  - Use Moq or NSubstitute for mocking dependencies.
  - Implement integration tests for API endpoints.
  
  ### Unit Testing with Mocking Best Practices
  - **Nullability in Mocks**: When setting up Mock returns for nullable reference types, use `.Returns(Task.FromResult(value))` instead of `.ReturnsAsync()` to avoid nullability compilation errors.
  - **Non-nullable Service Interfaces**: Always check service interface return types before mocking - if interface returns `Task<List<T>>`, don't try to return null, use empty collections instead.
  - **Optional Parameters in Mocks**: For methods with optional parameters, explicitly provide all parameters in mock setups and verifications to avoid "expression tree cannot contain optional arguments" errors.
  - **Pragma Directives**: Use `#nullable disable` and appropriate pragma warnings at the top of test files to handle complex nullability scenarios in mocking.
  - **Service Mock Consistency**: Ensure mock return types exactly match the service interface signatures, including nullability annotations.

  ### Controller Testing Anti-patterns to Avoid
  - **HTTP Response Types**: Don't expect specific action result types like `OkObjectResult` when controllers use API response wrappers - expect generic `ObjectResult` instead.
  - **Header Manipulation in Mocks**: Never use `Headers.Add()` in mock verifications - use indexer syntax `Headers["key"] = value` or avoid header verification in unit tests.
  - **Cache Service Mocking**: For cache services that return non-nullable types, always mock with empty collections rather than null to match actual service behavior.
  - **Expression Tree Limitations**: Avoid using `.Verify()` or `.Setup()` with methods that have optional parameters without explicitly specifying all parameter values.

  ### Test Data Setup Guidelines  
  - **Service Interface Alignment**: Before creating test mocks, read the actual service interfaces to understand exact return types and nullability.
  - **Controller Response Patterns**: Check how controllers wrap responses (e.g., `ApiResponseDto<T>`, `ApiErrorDto`) before writing test assertions.
  - **Mock Return Value Consistency**: Use realistic test data that matches the expected service behavior rather than null values that cause controller exception paths.
  - **Compilation Error Prevention**: After making multiple edits to test files, verify syntax integrity and rebuild before running tests to catch malformed generic types or incomplete method signatures.
  
  ### Integration Testing Best Practices
  - Always verify actual controller routes before writing integration tests - use grep/search to find exact endpoint patterns.
  - For WebApplicationFactory tests, ensure database seeding happens AFTER the web host is fully initialized, not during ConfigureWebHost.
  - Use CreateClient() method to trigger proper initialization before accessing database contexts in tests.
  - Implement thread-safe seeding with locks to prevent race conditions in parallel test execution.
  - Verify database context scopes are properly managed - avoid accessing disposed service providers.
  - Test actual HTTP endpoints rather than direct service calls to catch routing and middleware issues.

  ## Security
  - Implement proper CORS policies.

  ## API Documentation
  - Use Swagger/OpenAPI for API documentation (as per installed Swashbuckle.AspNetCore package).
  - Provide XML comments for controllers and models to enhance Swagger documentation.

  Follow the official Microsoft documentation and ASP.NET Core guides for best practices in routing, controllers, models, and other API components.



  ## Use consistent dotnet version throughout the project, preferably the latest LTS version & ensure all dependencies are compatible with that version.

  ## Use proper using statement before using any module class or method or dto so that it doesm't result in error in later stages

  ## Before using a Dto, class, method or interface check the name or properties inside it & then use it
  ## Avoid Using different name than whatever is written or devolped 

  ## Avoid using invalid properties that is not there in the codebase while suing dto , model ,class ,interface , methods , enum 

  ## AutoMapper Configuration Guidelines
  - Always create complete mappings for all properties - use explicit ForMember configurations for complex mappings.
  - When adding new DTOs or entities, immediately create corresponding AutoMapper profiles.
  - Use AssertConfigurationIsValid() only after ensuring ALL mappings are complete and tested.
  - For complex scenarios (e.g., List<Entity> to DTO), create custom value resolvers rather than ignoring properties.
  - Always map both directions (Entity to DTO and DTO to Entity) when bidirectional mapping is needed.
  
  ## Middleware Configuration Best Practices
  - Verify all required properties are set when configuring middleware (e.g., cache Size when SizeLimit is set).
  - Test middleware configuration in isolation before integrating into full pipeline.
  - Use proper error handling and logging in custom middleware to aid debugging.
  - Ensure middleware dependencies are properly registered in DI container before use.

  ## API Route Verification Protocol
  - Before writing integration tests, use file search to locate actual controller route attributes.
  - Verify route patterns match exactly between controllers and tests - include all route parameters.
  - Test both successful and error scenarios for each endpoint.
  - Use consistent route patterns across similar controllers (e.g., /api/{resource}/{id}/subresource).

  ## Database Context and Entity Framework Guidelines  
  - Always verify entity property types and names before creating DTOs or mappings.
  - Use proper scope management for DbContext in integration tests - create new scopes for each database operation.
  - Implement proper database seeding strategies for tests that don't interfere with test isolation.
  - When using in-memory databases, ensure data persistence across service scopes by proper initialization timing.

  ## Compilation Error Prevention for Test Files
  - **Nullability Reference Types**: When working with Mock frameworks and nullable reference types, use `#nullable disable` at the top of test files to avoid complex nullability compilation errors.
  - **Generic Type Syntax**: Always verify generic type syntax is complete and properly closed (e.g., avoid malformed patterns like `List<Seat>at>` or incomplete method signatures).
  - **Multiple File Edits**: After making multiple string replacements in a file, always run `dotnet build` to catch syntax corruption early before proceeding with test execution.
  - **Mock Setup Types**: Ensure mock setup return types exactly match the interface method signatures - use `Returns(Task.FromResult(value))` for complex async scenarios.
  - **Expression Tree Errors**: For methods with optional parameters, always provide explicit parameter values in mock expressions to avoid expression tree compilation errors.
