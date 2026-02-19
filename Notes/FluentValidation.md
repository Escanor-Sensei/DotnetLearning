# FluentValidation

## What is FluentValidation?

FluentValidation is a .NET library for building strongly-typed validation rules using a fluent interface and lambda expressions. Instead of decorating model properties with attributes (DataAnnotations), you define validation logic in separate validator classes using an expressive, readable syntax.

## Why FluentValidation over DataAnnotations?

| Aspect | DataAnnotations | FluentValidation |
|---|---|---|
| **Location** | Attributes on model properties | Separate validator classes |
| **Separation of concerns** | Validation mixed with model | Clean separation |
| **Complex logic** | Very limited (basic attributes only) | Full C# power (conditionals, async, cross-property) |
| **Testability** | Hard to unit test in isolation | Easy — just instantiate validator and call `Validate()` |
| **Custom messages** | Static strings only | Dynamic, runtime-computed messages |
| **Conditional rules** | Not supported | Built-in `.When()` / `.Unless()` |
| **Async validation** | Not supported | First-class `.MustAsync()` support |
| **Reusability** | Limited | Composable validators, shared rules |

## Core Concepts

### Validator Classes

Each DTO/model that needs validation gets its own validator class that inherits from `AbstractValidator<T>`. Rules are defined in the constructor using `RuleFor()`.

A validator class encapsulates all validation logic for a single type. This follows the Single Responsibility Principle — the model defines data shape, the validator defines validity constraints.

### The Validation Pipeline

```
HTTP Request → Model Binding → FluentValidation (automatic) → Controller Action
                                     ↓ (if invalid)
                              ModelState.IsValid = false → 400 Bad Request
```

When registered with ASP.NET Core, FluentValidation runs automatically during model binding. Invalid models populate `ModelState` with errors, and the API Behavior convention returns `400 Bad Request` automatically before your controller code even executes.

### Validation Results

A `ValidationResult` contains:
- `IsValid` — boolean indicating overall validity
- `Errors` — collection of `ValidationFailure` objects, each with:
  - `PropertyName` — which property failed
  - `ErrorMessage` — human-readable error description
  - `AttemptedValue` — the value that was rejected
  - `ErrorCode` — machine-readable error identifier

## Built-In Validators

FluentValidation provides a rich set of built-in validators:

### String Validators

| Validator | Purpose |
|---|---|
| `NotEmpty()` | Not null, not empty string, not whitespace |
| `NotNull()` | Not null (but allows empty string) |
| `Length(min, max)` | String length within range |
| `MinimumLength(n)` | At least N characters |
| `MaximumLength(n)` | At most N characters |
| `EmailAddress()` | Valid email format |
| `Matches(regex)` | Matches regular expression pattern |

### Numeric Validators

| Validator | Purpose |
|---|---|
| `GreaterThan(n)` | Value > n |
| `GreaterThanOrEqualTo(n)` | Value >= n |
| `LessThan(n)` | Value < n |
| `LessThanOrEqualTo(n)` | Value <= n |
| `InclusiveBetween(a, b)` | a <= value <= b |
| `ExclusiveBetween(a, b)` | a < value < b |
| `PrecisionScale(p, s)` | Decimal precision and scale |

### General Validators

| Validator | Purpose |
|---|---|
| `Equal(value)` | Must equal a specific value or another property |
| `NotEqual(value)` | Must not equal a value |
| `IsInEnum()` | Value must be a defined enum member |
| `Must(predicate)` | Custom synchronous validation logic |
| `MustAsync(predicate)` | Custom asynchronous validation logic |

## Advanced Validation Concepts

### Conditional Validation

Rules that only apply in certain situations:

- `.When(condition)` — Rule only runs if condition is true
- `.Unless(condition)` — Rule only runs if condition is false
- Conditions receive the model instance, so you can check other property values

**Use case**: "Company name is required only for business accounts" — use `.When(x => x.AccountType == AccountType.Business)`

### Cross-Property Validation

Validating relationships between multiple properties:
- `.Equal(x => x.OtherProperty)` — Password confirmation must match password
- `.GreaterThan(x => x.StartDate)` — End date must be after start date
- `.Must()` with a predicate that accesses the full object

### Async Validation

For rules that need to check external resources (database, API):
- `.MustAsync(async (value, cancellation) => ...)` — Custom async predicate
- Receives a `CancellationToken` for proper async cancellation
- **Use sparingly** — every async rule adds latency to validation

**Common use case**: Checking uniqueness (e.g., email not already in database)

### Collection Validation

| Method | Purpose |
|---|---|
| `RuleFor(x => x.Items)` | Validate the collection itself (NotEmpty, count, etc.) |
| `RuleForEach(x => x.Items)` | Apply rules to each element in the collection |
| `.SetValidator(new ItemValidator())` | Apply a child validator to complex child objects |

### RuleSets

Group rules into named sets that run only when explicitly invoked:
- Define: `RuleSet("Create", () => { RuleFor(...) })`
- Invoke: `validator.Validate(model, options => options.IncludeRuleSets("Create"))`
- Use cases: Different rules for create vs update, or different validation levels (quick vs thorough)

## Custom Validators

### Extension Method Validators

Create reusable validation rules as extension methods on `IRuleBuilder<T, TProperty>`. This lets you chain custom rules like built-in ones: `RuleFor(x => x.Title).MustBeValidTaskTitle()`.

### Property Validators

Create a class inheriting from `PropertyValidator<T, TProperty>` for more complex reusable validators that need access to the full validation context. Override `IsValid()` with your logic.

### When to Use Which

| Approach | Use When |
|---|---|
| Inline `.Must()` | One-off rule, only used in one validator |
| Extension method | Reusable rule across multiple validators |
| `PropertyValidator<T, TProperty>` | Complex rule needing validation context, custom error templates |

## Error Message Customization

### Static Messages
`.WithMessage("Title is required")` — Simple static string

### Parameterized Messages
`.WithMessage("'{PropertyName}' must be at least {MinLength} characters")` — Uses built-in placeholders

### Dynamic Messages
`.WithMessage(x => $"User {x.Username} is not valid")` — Runtime-computed from model values

### Error Codes
`.WithErrorCode("TITLE_REQUIRED")` — Machine-readable code for programmatic error handling on the client side

### Property Name Override
`.OverridePropertyName("title")` — Change the property name in error messages (useful when JSON property names differ from C# names)

## Integration with ASP.NET Core

### Automatic Validation

Two ways to integrate:
1. **Auto-validation** — Register `AddFluentValidationAutoValidation()` to run validators during model binding automatically
2. **Manual validation** — Inject `IValidator<T>` and call `Validate()` / `ValidateAsync()` explicitly in controller or service

### Custom Error Response

The default ASP.NET Core behavior returns `ProblemDetails` format for validation errors. You can customize this by configuring `ApiBehaviorOptions.InvalidModelStateResponseFactory` to return your own error response format.

### Validator Registration

- `AddValidatorsFromAssemblyContaining<T>()` — Scans and registers all validators from the assembly
- Default lifetime is **scoped** (new instance per request)
- Can register as **singleton** if validators have no injected dependencies
- Validators can accept DI services in their constructors (e.g., `IUserService` for uniqueness checks)

## FluentValidation vs Other Approaches

| Approach | Strengths | Weaknesses |
|---|---|---|
| **FluentValidation** | Powerful, testable, clean separation | Extra library dependency |
| **DataAnnotations** | Built-in, simple, familiar | Limited logic, hard to test |
| **Manual validation** | Full control, no dependencies | Repetitive, no standardization |
| **Custom `IValidatableObject`** | Built-in .NET, model-level | Still coupled to model, limited |

## Localization

FluentValidation supports localized error messages through:
- **`IStringLocalizer<T>`** injection into validator constructors
- **Resource files** (.resx) with translated messages
- **Built-in language support** — FluentValidation includes default messages in many languages

## Performance Considerations

- Validators are typically **scoped** (one per request) — registration as **singleton** avoids repeated allocations when validators have no scoped dependencies
- **Async rules** add overhead — use synchronous `.Must()` when no I/O is needed
- **CascadeMode** — Configure `RuleLevelCascadeMode = CascadeMode.Stop` to stop running rules for a property after the first failure (saves processing time)
- `.ValidateAndThrow()` is convenient but uses exceptions for flow control — prefer `Validate()` and check `IsValid` in performance-sensitive paths

## Migration from DataAnnotations

The migration path is straightforward:
1. Remove `[Required]`, `[StringLength]`, etc. attributes from the model
2. Create a corresponding `AbstractValidator<T>` class
3. Translate each attribute to its fluent equivalent
4. Register validators in the DI container
5. Models become clean POCOs — just properties, no validation logic

## Best Practices

### Do
- **One validator per model** — keeps validators focused and maintainable
- **Separate validator classes** from models — never put validation in the model itself
- **Test validators independently** — they're just classes, easy to unit test
- **Use meaningful error messages** — include property names and constraints
- **Keep validators simple** — extract complex logic into private methods or custom validators
- **Use `.CascadeMode`** — stop on first error to avoid confusing multiple error messages

### Don't
- **Heavy I/O in validators** — database calls on every validation slow down the API
- **Duplicate validation logic** — extract shared rules into custom validators or extension methods
- **Validate in multiple places** — pick one layer (usually the API boundary) for validation
- **Ignore validation in tests** — test both valid and invalid inputs for every validator

---

*FluentValidation transforms validation from scattered attribute checking into a centralized, testable, and expressive validation system. Its fluent API makes complex business rules readable while maintaining clean separation from your data models.*