# FluentValidation - Elegant Validation Rules Guide

## 🔍 **What is FluentValidation?**

FluentValidation is a .NET library that uses a **fluent interface** and **lambda expressions** for building strongly-typed validation rules. It provides a clean, expressive way to define complex validation logic.

## 🎯 **Why Use FluentValidation?**

### **Problems with DataAnnotations:**
- ❌ Limited validation logic (basic attributes only)
- ❌ Hard to test validation rules in isolation
- ❌ Difficult to create complex, conditional validations
- ❌ No easy way to customize error messages dynamically
- ❌ Tight coupling between models and validation logic

### **FluentValidation Solutions:**
- ✅ **Rich Validation Rules** - Complex logic with easy syntax
- ✅ **Separation of Concerns** - Validators are separate classes
- ✅ **Easy Testing** - Unit test validation rules independently
- ✅ **Conditional Validation** - Rules based on other properties
- ✅ **Custom Error Messages** - Dynamic, localized messages
- ✅ **Extensible** - Create custom validation rules easily

## 🏗️ **How FluentValidation Works**

```
Request → Model Binding → FluentValidation → Controller Action
                            ↓ (if invalid)
                        ValidationException → Error Response
```

1. **Define Validators** - Create validator classes for your models
2. **Register Validators** - Add to DI container
3. **Automatic Validation** - ASP.NET Core runs validation automatically
4. **Handle Results** - Get validation errors in ModelState

## 📚 **Basic Implementation**

### **1. Install NuGet Package**
```xml
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
```

### **2. Create a Validator Class**
```csharp
public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .Length(3, 100).WithMessage("Title must be between 3 and 100 characters");
            
        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value");
            
        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.Now)
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future");
    }
}
```

### **3. Register in DI Container**
```csharp
// Program.cs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskValidator>();
```

### **4. Use in Controller**
```csharp
[HttpPost]
public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
{
    // Validation happens automatically
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    
    // Process valid data
    var task = await _taskService.CreateTaskAsync(dto);
    return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
}
```

## 🔧 **Common Validation Rules**

### **String Validations**
```csharp
RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress()
    .WithMessage("Valid email address is required");

RuleFor(x => x.Password)
    .NotEmpty()
    .MinimumLength(8)
    .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
    .WithMessage("Password must contain uppercase, lowercase, and number");

RuleFor(x => x.PhoneNumber)
    .NotEmpty()
    .Matches(@"^\+?[1-9]\d{1,14}$")
    .WithMessage("Invalid phone number format");
```

### **Number Validations**
```csharp
RuleFor(x => x.Age)
    .GreaterThanOrEqualTo(18)
    .LessThan(100)
    .WithMessage("Age must be between 18 and 100");

RuleFor(x => x.Price)
    .GreaterThan(0)
    .PrecisionScale(10, 2, false)
    .WithMessage("Price must be positive with max 2 decimal places");
```

### **Collection Validations**
```csharp
RuleFor(x => x.Tags)
    .NotEmpty()
    .WithMessage("At least one tag is required");

RuleForEach(x => x.Tags)
    .NotEmpty()
    .Length(2, 20)
    .WithMessage("Each tag must be 2-20 characters");
```

### **Date/Time Validations**
```csharp
RuleFor(x => x.StartDate)
    .NotEmpty()
    .GreaterThanOrEqualTo(DateTime.Today)
    .WithMessage("Start date cannot be in the past");

RuleFor(x => x.EndDate)
    .GreaterThan(x => x.StartDate)
    .WithMessage("End date must be after start date");
```

## 🎛️ **Advanced Features**

### **Conditional Validation**
```csharp
RuleFor(x => x.CompanyName)
    .NotEmpty()
    .When(x => x.UserType == UserType.Business)
    .WithMessage("Company name is required for business users");

RuleFor(x => x.VatNumber)
    .NotEmpty()
    .Unless(x => x.Country == "US")
    .WithMessage("VAT number is required for non-US customers");
```

### **Complex Validations**
```csharp
RuleFor(x => x)
    .Must(BeValidBusinessUser)
    .WithMessage("Business users must have valid company information");

private bool BeValidBusinessUser(CreateUserDto dto)
{
    if (dto.UserType != UserType.Business)
        return true; // Skip validation for non-business users
        
    return !string.IsNullOrEmpty(dto.CompanyName) 
           && !string.IsNullOrEmpty(dto.VatNumber)
           && dto.CompanyName.Length >= 3;
}
```

### **Async Validation**
```csharp
RuleFor(x => x.Email)
    .MustAsync(BeUniqueEmail)
    .WithMessage("Email address is already in use");

private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
{
    var user = await _userService.GetByEmailAsync(email);
    return user == null;
}
```

### **Cross-Property Validation**
```csharp
RuleFor(x => x.ConfirmPassword)
    .Equal(x => x.Password)
    .WithMessage("Passwords do not match");

RuleFor(x => x)
    .Must(x => x.EndDate > x.StartDate)
    .WithMessage("End date must be after start date")
    .OverridePropertyName("EndDate");
```

## 🧪 **Testing Validation**

### **Unit Testing Validators**
```csharp
[Test]
public void Should_Have_Error_When_Title_Is_Empty()
{
    // Arrange
    var model = new CreateTaskDto { Title = "" };
    var validator = new CreateTaskValidator();
    
    // Act
    var result = validator.TestValidate(model);
    
    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Title)
          .WithErrorMessage("Task title is required");
}

[Test]
public void Should_Not_Have_Error_When_Valid()
{
    // Arrange
    var model = new CreateTaskDto 
    { 
        Title = "Valid Task",
        Priority = TaskPriority.Medium 
    };
    var validator = new CreateTaskValidator();
    
    // Act
    var result = validator.TestValidate(model);
    
    // Assert
    result.ShouldNotHaveAnyValidationErrors();
}
```

### **Integration Testing**
```csharp
[Test]
public async Task CreateTask_WithInvalidData_ReturnsBadRequest()
{
    // Arrange
    var invalidTask = new CreateTaskDto { Title = "" };
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/tasks", invalidTask);
    
    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    
    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("Task title is required", content);
}
```

## 🎨 **Custom Validators**

### **Create Custom Rule**
```csharp
public static class CustomValidators
{
    public static IRuleBuilderOptions<T, string> MustBeValidTaskTitle<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .Must(title => !title.StartsWith(" ") && !title.EndsWith(" "))
            .Must(title => !title.Contains("  ")) // No double spaces
            .WithMessage("Task title must not have leading/trailing spaces or double spaces");
    }
}

// Usage
RuleFor(x => x.Title).MustBeValidTaskTitle();
```

### **Property Validator**
```csharp
public class NoSwearWordsValidator<T> : PropertyValidator<T, string>
{
    private readonly string[] _swearWords = { "bad", "worse", "terrible" };
    
    public override bool IsValid(ValidationContext<T> context, string value)
    {
        if (string.IsNullOrEmpty(value))
            return true;
            
        return !_swearWords.Any(word => 
            value.Contains(word, StringComparison.OrdinalIgnoreCase));
    }
    
    public override string Name => "NoSwearWordsValidator";
    
    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return "'{PropertyName}' contains inappropriate language";
    }
}
```

## 📊 **Error Handling & Responses**

### **Default Error Response**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": ["Task title is required"],
    "Priority": ["Invalid priority value"]
  }
}
```

### **Custom Error Response**
```csharp
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new ValidationError
                {
                    Field = x.Key,
                    Messages = x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                })
                .ToArray();

            var response = new ValidationErrorResponse
            {
                Message = "Validation failed",
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };

            context.Result = new BadRequestObjectResult(response);
        }
    }
    
    public void OnActionExecuted(ActionExecutedContext context) { }
}
```

## 🌍 **Localization**

### **Localized Messages**
```csharp
public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator(IStringLocalizer<CreateTaskValidator> localizer)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(x => localizer["TaskTitleRequired"])
            .Length(3, 100)
            .WithMessage(x => localizer["TaskTitleLength", 3, 100]);
    }
}
```

## 📈 **Performance Considerations**

### **DO's ✅**
- ✅ **Cache Validators** - Register as Singleton when possible
- ✅ **Use Async Sparingly** - Only when accessing external resources
- ✅ **Validate Early** - Stop on first failure for expensive validations
- ✅ **Use RuleSets** - Group related validations

### **DON'Ts ❌**
- ❌ **Don't Over-Validate** - Balance between security and performance
- ❌ **Avoid Heavy Database Calls** - In every validation rule
- ❌ **Don't Duplicate Logic** - Keep validation DRY

## 🔄 **Migration from DataAnnotations**

### **Before (DataAnnotations)**
```csharp
public class CreateTaskDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; }
    
    [Range(1, 4, ErrorMessage = "Priority must be 1-4")]
    public TaskPriority Priority { get; set; }
}
```

### **After (FluentValidation)**
```csharp
// Clean DTO without validation attributes
public class CreateTaskDto
{
    public string Title { get; set; }
    public TaskPriority Priority { get; set; }
}

// Separate validator class
public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(3, 100);
            
        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value");
    }
}
```

## 🎓 **Learning Benefits**

### **For Developers:**
1. 🧹 **Clean Code** - Separation of concerns between models and validation
2. 🧪 **Testability** - Easy to unit test validation logic
3. 🔧 **Flexibility** - Complex conditional validations
4. 📖 **Readability** - Fluent syntax is self-documenting

### **For Applications:**
1. 🔒 **Data Integrity** - Robust validation ensures clean data
2. 👥 **User Experience** - Better error messages and handling
3. 🎯 **Business Rules** - Complex validation logic implementation
4. 🌐 **Internationalization** - Easy localization support

## 🚀 **Quick Setup Summary**

1. **Add Package** ➜ FluentValidation.AspNetCore
2. **Create Validators** ➜ Inherit from AbstractValidator<T>
3. **Define Rules** ➜ Use RuleFor() with fluent syntax
4. **Register Services** ➜ AddFluentValidationAutoValidation()
5. **Handle Results** ➜ Check ModelState.IsValid
6. **Test Validators** ➜ Use TestValidate() method

---

*FluentValidation transforms validation from simple attribute checking into a powerful, flexible validation framework. Start with basic rules, then gradually add more complex logic as your application grows.*