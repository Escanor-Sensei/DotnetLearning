# FluentValidation Testing

## Why Test Validators?

Validators define business rules — they determine what data enters your system. Bugs in validation lead to:
- **Invalid data in the database** — corrupted business state
- **False rejections** — valid requests blocked, poor user experience
- **Security gaps** — missing validation on dangerous inputs

Since validators are simple classes with no infrastructure dependencies, they're among the easiest components to unit test. There's no excuse for untested validators.

## Testing Approaches

### 1. Direct Validation (Recommended)

Instantiate the validator directly, call `Validate()` with a model, and assert on the `ValidationResult`.

**Pattern:**
```
var validator = new CreateTaskValidator();
var result = validator.Validate(model);
// Assert: result.IsValid, result.Errors.Count, error messages, property names
```

**Advantages:**
- No extra dependencies needed
- Full control over assertion logic
- Works with any test framework
- Easy to understand

### 2. TestValidate Helper (FluentValidation.TestHelper)

FluentValidation provides a `TestHelper` package with extension methods for more expressive test assertions:
- `validator.TestValidate(model)` — returns a `TestValidationResult`
- `.ShouldHaveValidationErrorFor(x => x.PropertyName)` — asserts specific property failed
- `.ShouldNotHaveValidationErrorFor(x => x.PropertyName)` — asserts specific property passed
- `.WithErrorMessage("expected message")` — chains to assert on the error message
- `.ShouldNotHaveAnyValidationErrors()` — asserts everything passed

**Trade-off:** Adds a NuGet dependency (`FluentValidation.TestHelper`) but makes tests more readable and specific.

### 3. Integration Testing

Test validation through the full HTTP pipeline to verify:
- Validators are correctly registered in DI
- Auto-validation is wired up properly
- Error responses have the expected format
- Multiple validators interact correctly

## Test Categories for Validators

### Positive Tests (Valid Input)

Confirm that valid inputs pass validation:
- All required fields provided with valid values
- Boundary values (minimum length, maximum length, edge dates)
- Optional fields omitted (should still pass)
- Different valid formats (e.g., various email formats)

### Negative Tests (Invalid Input)

Confirm that invalid inputs fail with correct errors:
- Required fields empty, null, or whitespace
- Values exceeding maximum limits
- Values below minimum limits
- Invalid formats (bad email, wrong regex)
- Business rule violations (past dates, negative amounts)

### Conditional Rule Tests

Test rules that depend on other properties:
- Verify rule fires when the condition is met
- Verify rule does NOT fire when the condition is not met
- Test boundary conditions of the condition itself

### Error Message Tests

Verify error messages are correct and helpful:
- Message text matches expected wording
- Property name is correctly identified
- Custom error codes are set when applicable

## Test Organization Strategies

### One Test Class Per Validator

```
ValidatorTests/
├── CreateTaskDtoValidatorTests.cs
├── UpdateTaskDtoValidatorTests.cs
├── LoginDtoValidatorTests.cs
└── UserRegistrationDtoValidatorTests.cs
```

Each test class focuses on a single validator, making failures easy to trace.

### Test Naming Convention

```
[Target]_[Scenario]_[ExpectedResult]

Examples:
- Title_WhenEmpty_ShouldFail
- Title_WhenValidLength_ShouldPass
- DueDate_WhenInPast_ShouldFail
- Priority_WhenCriticalWithNoDescription_ShouldFail
```

### Shared Test Data

Use builder patterns or static test data classes for consistent test inputs across multiple tests. This avoids duplicating test object construction in every test method.

## Parameterized Testing

Test multiple input scenarios with a single test method:

**Theory/InlineData approach (xUnit):**
- `[Theory]` marks a test that runs multiple times
- `[InlineData("")]`, `[InlineData(null)]` — each set becomes a separate test run
- Ideal for testing the same rule with many different invalid (or valid) inputs

**MemberData approach:**
- For complex objects that can't be passed as `InlineData`
- Define a static method/property that returns test cases
- Each test case is a set of parameters

## Testing Async Validators

Validators with `.MustAsync()` rules require special handling:
- Call `ValidateAsync()` instead of `Validate()`
- Mock the async dependency (e.g., `IUserService` for uniqueness check)
- Test both the "exists" and "doesn't exist" paths
- Verify `CancellationToken` is properly forwarded

## Testing Validators with DI Dependencies

Some validators inject services (e.g., database check for uniqueness):
- **Unit test:** Mock the injected service, pass the mock to the validator constructor
- **Integration test:** Let the DI container resolve everything, use a real (in-memory) database
- Keep validators' DI dependencies minimal — prefer simple validators that don't need external services

## Common Testing Mistakes

| Mistake | Why It's Wrong | Better Approach |
|---|---|---|
| Only testing the happy path | Misses all validation failures | Test every rule with at least one invalid input |
| Testing FluentValidation itself | `NotEmpty()` already works — you don't need to prove it | Test YOUR rules and configurations |
| Hardcoding expected error counts | Brittle — adding a rule breaks existing tests | Assert on specific properties and messages |
| Not testing conditional rules | Conditional bugs are the hardest to find | Test with condition true AND false |
| Ignoring error messages | Users see these messages | Verify messages are correct and helpful |
| Testing validators through controllers | Slow, complex setup, tests multiple things | Test validators directly — fast and isolated |

## Automatic vs Manual Validation Testing

### Automatic Validation (Integration Concern)
When FluentValidation is registered with `AddFluentValidationAutoValidation()`, validation runs automatically during model binding. Integration tests should verify:
- Invalid requests return 400 Bad Request
- Error response body contains expected validation messages
- Valid requests proceed to the controller action

### Manual Validation (Explicit Call)
When validators are called explicitly via `IValidator<T>.Validate()` in service/controller code, you have more control but need unit tests to verify the validator is actually being called.

## Relationship Between Validation and Error Responses

The validation result flows through several layers:

```
FluentValidation → ModelState → API Behavior Convention → HTTP Response

ValidationFailure {          ModelState {             Response {
  PropertyName: "Title"  →     "Title": ["required"]  →   Status: 400
  ErrorMessage: "required"                                 Errors: { "Title": ["required"] }
}                            }                           }
```

Customizing any layer changes the error format:
- **FluentValidation:** Controls what errors exist and their messages
- **API Behavior Options:** Controls how ModelState errors become HTTP responses
- **Custom Action Filters:** Can intercept and reformat before the response is sent

---

*Validator testing is one of the highest-value, lowest-effort testing activities. Validators are pure logic with no infrastructure dependencies — test them thoroughly to ensure your business rules are correctly enforced.*