# Unit Testing with xUnit and Moq

## Overview
Unit testing is a fundamental software quality practice that validates individual components in isolation. It forms the foundation of reliable, maintainable software by ensuring each unit behaves correctly under various conditions.

## Core Testing Theory

### 1. FIRST Principles
- **Fast**: Tests execute quickly (milliseconds, not seconds)
- **Independent**: Tests don't depend on other tests or external state
- **Repeatable**: Same results in any environment
- **Self-Validating**: Clear pass/fail outcome
- **Timely**: Written alongside or before production code

### 2. Testing Pyramid Strategy
```
    /\
   /  \    E2E Tests (Few, Expensive, Slow)
  /____\
 /      \   Integration Tests (Some, Moderate Cost)
/________\  Unit Tests (Many, Fast, Cheap)
```

**Philosophy**: Many fast unit tests, fewer integration tests, minimal E2E tests.

### 3. Test Structure Patterns

#### AAA Pattern (Arrange-Act-Assert)
- **Arrange**: Setup test prerequisites and inputs
- **Act**: Execute the unit under test
- **Assert**: Verify expected outcomes

#### Given-When-Then (BDD Style)
- **Given**: Initial context/state
- **When**: Event occurs
- **Then**: Expected outcome

## xUnit Framework Concepts

### 1. Test Categories
- **[Fact]**: Single test with no parameters
- **[Theory]**: Parameterized test with multiple data sets
- **[Skip]**: Temporarily disable tests
- **Traits**: Categorize and filter tests

### 2. Test Lifecycle Management
- **Constructor**: Per-test setup (fresh instance each test)
- **IDisposable**: Per-test cleanup
- **IClassFixture**: Shared setup across test class
- **ICollectionFixture**: Shared setup across test classes

### 3. Test Organization Strategies
- Group related tests in nested classes
- Use descriptive naming conventions
- Organize by feature, not by technical layer
- Maintain one assertion per test (generally)

## Moq Framework Theory

### 1. Test Doubles Hierarchy
- **Dummy**: Objects passed but never used
- **Fake**: Working implementations with shortcuts (in-memory database)
- **Stub**: Provides canned answers to calls
- **Spy**: Records information about how they were called
- **Mock**: Pre-programmed with expectations of calls they expect to receive

### 2. Mocking Strategies
- **Loose Mocks**: Return default values for unexpected calls
- **Strict Mocks**: Throw exceptions for unexpected calls
- **Partial Mocks**: Mock some methods while keeping others real

### 3. Verification Patterns
- **State Verification**: Assert on state changes result from actions
- **Behavior Verification**: Verify interactions occurred as expected
- **Never**: Ensure methods were not called inappropriately

## Testing Architecture Layers

### 1. Controller Testing Strategy
**Focus**: HTTP concerns, routing, model binding, status codes
**Dependencies**: Mock services, avoid databases/external systems
**Verify**: Correct HTTP responses, proper service calls, error handling

### 2. Service Layer Testing Strategy  
**Focus**: Business logic, validation, orchestration
**Dependencies**: Mock repositories, external services
**Verify**: Business rules, exception handling, workflow correctness

### 3. Repository Testing Strategy
**Options**: 
- In-memory databases for integration-style tests
- Mock IDbConnection for pure unit tests
- Test actual SQL with containerized databases

### 4. Authentication Testing Approach
**JWT Testing**: Mock token generation/validation
**Authorization**: Test claim-based access control
**Integration**: Test full auth flow with test server

## Integration Testing Concepts

### 1. Test Server Strategy
**Purpose**: Test complete HTTP pipeline without network calls
**Benefits**: Fast execution, controlled environment, full feature testing
**Trade-offs**: Not testing actual deployment configuration

### 2. Database Testing Approaches
- **In-Memory**: Fast, isolated, may not reflect production behavior
- **Test Containers**: Realistic database, slower setup, requires Docker
- **Shared Test DB**: Fast, shared state issues, cleanup complexity

### 3. External Service Testing
- **Mock External APIs**: Fast, predictable, doesn't test integration
- **Test Doubles**: Controllable external service behavior
- **Contract Testing**: Verify external service compatibility

## Validation Testing Patterns

### 1. FluentValidation Testing Strategy
**Approaches**:
- Direct validator testing for business rules
- Integration testing for pipeline validation
- Custom validator unit tests

**Focus Areas**:
- Conditional validation logic
- Cross-field validation rules
- Localization and error messages

### 2. Model Binding Testing
**Concepts**:
- Invalid model state handling
- Custom model binders
- Validation attribute testing

## Test Data Management Patterns

### 1. Object Builder Pattern
**Benefits**: Fluent API, default values, easy test data creation
**Implementation**: Chainable methods, implicit conversion, reusable builders

### 2. Object Mother Pattern  
**Purpose**: Named factory methods for common object configurations
**Use Cases**: Standard scenarios, complex object graphs, domain concepts

### 3. Test Data Categories
- **Minimal Valid**: Smallest possible valid object
- **Typical**: Representative real-world data
- **Edge Cases**: Boundary values, edge conditions
- **Invalid**: Data that should cause exceptions/validation failures

## Async Testing Considerations

### 1. Async vs Sync Testing
**Key Points**:
- Always await async methods in tests
- Test both happy path and exception scenarios
- Verify async operations complete properly
- Consider timeout scenarios

### 2. Concurrency Testing
**Concepts**:
- Thread safety verification
- Race condition detection
- Lock contention testing
- Async enumerable testing

## Testing Performance and Quality

### 1. Performance Testing in Unit Tests
**Approaches**:
- Benchmark critical paths
- Memory allocation testing  
- Timeout verification
- Performance regression detection

### 2. Code Coverage Concepts
**Metrics**:
- Line Coverage: Percentage of executed lines
- Branch Coverage: Percentage of executed branches
- Path Coverage: Percentage of execution paths tested

**Guidelines**:
- Aim for 80%+ coverage on business logic
- 100% coverage ≠ 100% quality
- Focus on critical and complex code paths

## Testing Best Practices

### 1. Test Naming Strategies
**Approaches**:
- **Method_Scenario_ExpectedBehavior**: `CreateTask_WithEmptyTitle_ThrowsValidationException`
- **Should_Behavior_When_Condition**: `Should_ReturnFalse_When_TaskNotFound`
- **Given_When_Then**: `Given_ValidUser_When_CreatingTask_Then_TaskIsCreated`

### 2. Test Organization Patterns
- Group related tests in nested classes by feature
- Use descriptive test class names that explain the system under test
- Organize tests by business scenarios, not technical implementation
- Maintain consistent test structure across the codebase

### 3. Mock Management Principles
- **Setup Reusability**: Common setups in constructors, specific overrides in tests
- **Verification Strategy**: Verify important interactions, avoid over-verification
- **Mock Scope**: One mock per dependency interface, reset between tests

### 4. Test Data Strategy
- **Consistency**: Use builders for complex objects, constants for simple values
- **Clarity**: Make test data intention obvious through naming
- **Maintainability**: Centralize common test data, avoid duplication
- **Isolation**: Each test should create its own data to avoid dependencies

### 5. Error Testing Approach
- Test both exception types and messages for specificity
- Verify error handling at appropriate abstraction levels
- Test error propagation through the system
- Include edge cases and boundary conditions

### 6. Test Maintenance Guidelines
- Keep tests simple and focused on single concerns
- Refactor tests when production code changes
- Remove or fix consistently failing tests quickly
- Document complex test scenarios with comments

## Test Configuration and Tooling

### 1. Project Structure
```
TestProject/
├── Unit/
│   ├── Controllers/
│   ├── Services/
│   └── Repositories/
├── Integration/
├── Builders/
├── Fixtures/
└── TestData/
```

### 2. Essential NuGet Packages
- **Microsoft.NET.Test.Sdk**: Test platform foundation
- **xunit**: Primary testing framework
- **xunit.runner.visualstudio**: Visual Studio integration
- **Moq**: Mocking framework for isolating dependencies
- **FluentValidation**: For validation testing
- **Microsoft.AspNetCore.Mvc.Testing**: Web API integration testing
- **Microsoft.EntityFrameworkCore.InMemory**: Database testing

### 3. Test Categorization Strategy
Use traits and filters to organize test execution:
- **Category**: Unit, Integration, Performance
- **Feature**: Authentication, TaskManagement, Reporting  
- **Priority**: Critical, High, Medium, Low
- **Environment**: Fast, Slow, External-Dependencies

### 4. Continuous Integration Considerations
- **Test Execution**: Fast unit tests first, slower tests later
- **Parallel Execution**: Design tests to run independently
- **Test Data**: Isolated databases/data per test run
- **Reporting**: Generate coverage reports and test results

## Advanced Testing Concepts

### 1. Test-Driven Development (TDD)
**Red-Green-Refactor Cycle**:
1. Write failing test (Red)
2. Write minimal code to pass (Green) 
3. Improve code quality (Refactor)

**Benefits**: Better design, comprehensive coverage, documentation

### 2. Behavior-Driven Development (BDD)
**Focus**: Business behavior over implementation details
**Language**: Given-When-Then scenarios
**Tools**: SpecFlow, BDDfy for .NET

### 3. Property-Based Testing
**Concept**: Generate test inputs automatically
**Tools**: FsCheck for .NET
**Use Cases**: Testing invariants with large input spaces

### 4. Mutation Testing
**Purpose**: Test the quality of your tests
**Process**: Introduce small code changes and verify tests fail
**Tools**: Stryker.NET for mutation testing

## Testing Anti-Patterns to Avoid

### 1. Common Pitfalls
- **Testing Implementation Details**: Focus on behavior, not internal structure
- **Fragile Tests**: Avoid tests that break with minor refactoring
- **Test Dependencies**: Each test should run independently
- **Over-Mocking**: Don't mock everything; use real objects when beneficial
- **Assertion Overload**: One logical assertion per test

### 2. Test Smells
- **Long Test Methods**: Break into smaller, focused tests
- **Magic Numbers**: Use meaningful constants
- **Duplicate Setup**: Extract common setup to builder methods
- **Unclear Intent**: Test names and structure should tell a story

### 3. Maintenance Issues
- **Copy-Paste Tests**: Use parameterized tests or test builders
- **Outdated Tests**: Keep tests synchronized with requirements
- **Slow Test Suite**: Optimize or categorize slow tests separately

This theoretical foundation provides the conceptual framework for effective unit testing with xUnit and Moq, focusing on principles and patterns rather than specific implementation details.