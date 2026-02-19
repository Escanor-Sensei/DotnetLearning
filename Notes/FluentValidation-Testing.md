# FluentValidation Testing Examples

## Testing Without xUnit (Console Examples)

Since this is a Web API project, here are simple console examples to test validators manually:

```csharp
// Example: Testing CreateTaskItemDtoValidator in a console app or controller action

var validator = new CreateTaskItemDtoValidator();

// Test Case 1: Valid input
var validDto = new CreateTaskItemDto
{
    Title = "Valid Task Title",
    Description = "Valid description",
    Priority = TaskPriority.Medium,
    DueDate = DateTime.Now.AddDays(7)
};

var validResult = validator.Validate(validDto);
Console.WriteLine($"Valid DTO - IsValid: {validResult.IsValid}");

// Test Case 2: Invalid input - empty title
var invalidDto = new CreateTaskItemDto
{
    Title = "",
    Priority = TaskPriority.Medium
};

var invalidResult = validator.Validate(invalidDto);
Console.WriteLine($"Invalid DTO - IsValid: {invalidResult.IsValid}");
Console.WriteLine("Errors:");
foreach (var error in invalidResult.Errors)
{
    Console.WriteLine($"- {error.PropertyName}: {error.ErrorMessage}");
}

// Test Case 3: Critical task without description
var criticalDto = new CreateTaskItemDto
{
    Title = "Critical Task",
    Priority = TaskPriority.Critical,
    Description = null // Should fail validation
};

var criticalResult = validator.Validate(criticalDto);
Console.WriteLine($"Critical Task without Description - IsValid: {criticalResult.IsValid}");
Console.WriteLine("Errors:");
foreach (var error in criticalResult.Errors)
{
    Console.WriteLine($"- {error.PropertyName}: {error.ErrorMessage}");
}
```

## Manual Testing in Controller Actions

You can also test validators manually in your controller actions:

```csharp
[HttpPost("test-validation")]
public ActionResult TestValidation([FromBody] CreateTaskItemDto dto)
{
    var validator = new CreateTaskItemDtoValidator();
    var result = validator.Validate(dto);
    
    if (!result.IsValid)
    {
        return BadRequest(new 
        { 
            Message = "Validation failed",
            Errors = result.Errors.Select(e => new 
            { 
                Property = e.PropertyName,
                Message = e.ErrorMessage 
            })
        });
    }
    
    return Ok(new { Message = "Validation passed" });
}
```

## Testing Validation Behavior

### Test Cases for CreateTaskItemDto:

1. **Empty Title** - Should fail
2. **Title too long (>100 chars)** - Should fail
3. **Title with leading/trailing spaces** - Should fail
4. **Critical task without description** - Should fail
5. **High/Critical task without due date** - Should fail
6. **Due date in past** - Should fail
7. **Valid task** - Should pass

### Test Cases for LoginDto:

1. **Empty username** - Should fail
2. **Invalid username format** - Should fail
3. **Valid username (alphanumeric)** - Should pass
4. **Valid email as username** - Should pass
5. **Password too short (<6 chars)** - Should fail
6. **Valid password** - Should pass

## Setting Up a Proper Test Project

To create proper unit tests, you would:

1. Create a new test project:
   ```bash
   dotnet new xunit -n TaskManagementAPI.Tests
   ```

2. Add project references:
   ```bash
   cd TaskManagementAPI.Tests
   dotnet add reference ../TaskManagementAPI
   dotnet add package FluentValidation.TestHelper
   ```

3. Create test classes using the examples above.

## Automatic Validation in ASP.NET Core

With FluentValidation registered in Program.cs:
- Validation automatically occurs on model binding
- Invalid models return 400 BadRequest with validation errors
- No manual validation needed in controllers (for basic scenarios)

## Custom Validation Response

To customize validation responses, you can configure FluentValidation:

```csharp
// In Program.cs
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        return new BadRequestObjectResult(new
        {
            Message = "Validation failed",
            Errors = errors
        });
    };
});
```