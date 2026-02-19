using FluentValidation;
using TaskManagementAPI.Models;
using TaskManagementAPI.Models.DTOs;
using TaskManagementAPI.Tests.Builders;
using TaskManagementAPI.Validators;

namespace TaskManagementAPI.Tests.Validators;

public class CreateTaskItemDtoValidatorTests
{
    private readonly CreateTaskItemDtoValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        // Arrange
        var model = new CreateTaskItemDtoBuilder()
            .WithTitle("")
            .Build();

        // Act
        var result = _validator.Validate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(model.Title));
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Too_Short()
    {
        // Arrange
        var model = new CreateTaskItemDtoBuilder()
            .WithTitle("ab") // Too short
            .Build();

        // Act
        var result = _validator.Validate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(model.Title));
    }

    [Fact]
    public void Should_Have_Error_When_Critical_Task_Has_No_Description()
    {
        // Arrange
        var model = new CreateTaskItemDtoBuilder()
            .WithTitle("Critical Task")
            .WithPriority(TaskPriority.Critical)
            .WithDescription(null)
            .Build();

        // Act
        var result = _validator.Validate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(model.Description));
    }

    [Fact]
    public void Should_Not_Have_Error_When_Critical_Task_Has_Description()
    {
        // Arrange
        var model = new CreateTaskItemDtoBuilder()
            .WithTitle("Critical Task")
            .WithPriority(TaskPriority.Critical)
            .WithDescription("This is a critical task with proper description")
            .Build();

        // Act
        var result = _validator.Validate(model);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Due_Date_Is_In_Past()
    {
        // Arrange
        var model = new CreateTaskItemDtoBuilder()
            .WithTitle("Task with past due date")
            .WithDueDate(DateTime.Now.AddDays(-1))
            .Build();

        // Act
        var result = _validator.Validate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(model.DueDate));
    }

    [Fact]
    public void Should_Be_Valid_When_All_Rules_Pass()
    {
        // Arrange
        var model = new CreateTaskItemDtoBuilder()
            .WithTitle("Valid Task Title")
            .WithDescription("Valid description")
            .WithPriority(TaskPriority.Medium)
            .WithDueDate(DateTime.Now.AddDays(7))
            .Build();

        // Act
        var result = _validator.Validate(model);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}

public class LoginDtoValidatorTests
{
    private readonly LoginDtoValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Username_Is_Empty()
    {
        // Arrange
        var model = new LoginDto { Username = "", Password = "password" };

        // Act
        var result = _validator.Validate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(model.Username));
    }

    [Fact]
    public void Should_Have_Error_When_Username_Is_Too_Short()
    {
        // Arrange
        var model = new LoginDto { Username = "ab", Password = "password" }; // Too short

        // Act
        var result = _validator.Validate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(model.Username));
    }

    [Fact]
    public void Should_Not_Have_Error_When_Username_Is_Valid_Alphanumeric()
    {
        // Arrange
        var model = new LoginDto { Username = "validuser", Password = "password" };

        // Act
        var result = _validator.Validate(model);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Username_Is_Valid_Email()
    {
        // Arrange
        var model = new LoginDto { Username = "test@example.com", Password = "password" };

        // Act
        var result = _validator.Validate(model);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        // Arrange
        var model = new LoginDto { Username = "validuser", Password = "123" }; // Too short

        // Act
        var result = _validator.Validate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(model.Password));
    }

    [Fact]
    public void Should_Be_Valid_When_All_Rules_Pass()
    {
        // Arrange
        var model = new LoginDto
        {
            Username = "validuser",
            Password = "password123"
        };

        // Act
        var result = _validator.Validate(model);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}