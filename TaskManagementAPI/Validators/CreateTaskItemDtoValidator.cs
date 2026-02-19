using FluentValidation;
using TaskManagementAPI.Models;
using TaskManagementAPI.Models.DTOs;

namespace TaskManagementAPI.Validators;

public class CreateTaskItemDtoValidator : AbstractValidator<CreateTaskItemDto>
{
    public CreateTaskItemDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .Length(3, 100).WithMessage("Title must be between 3 and 100 characters")
            .Must(NotHaveLeadingOrTrailingSpaces).WithMessage("Title cannot have leading or trailing spaces")
            .Must(NotContainOnlyWhitespace).WithMessage("Title cannot contain only whitespace characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .Must(NotContainOnlyWhitespace)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot contain only whitespace characters");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value. Must be Low (1), Medium (2), High (3), or Critical (4)");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.Now)
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future");

        // Complex business rule: Critical tasks must have a description
        RuleFor(x => x.Description)
            .NotEmpty()
            .When(x => x.Priority == TaskPriority.Critical)
            .WithMessage("Critical priority tasks must have a description");

        // Business rule: High/Critical tasks should have a due date
        RuleFor(x => x.DueDate)
            .NotNull()
            .When(x => x.Priority == TaskPriority.High || x.Priority == TaskPriority.Critical)
            .WithMessage("High and Critical priority tasks should have a due date");
    }

    private static bool NotHaveLeadingOrTrailingSpaces(string title)
    {
        if (string.IsNullOrEmpty(title))
            return true;

        return title == title.Trim();
    }

    private static bool NotContainOnlyWhitespace(string value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        return !string.IsNullOrWhiteSpace(value);
    }
}