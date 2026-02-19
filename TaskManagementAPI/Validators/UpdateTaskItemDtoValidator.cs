using FluentValidation;
using TaskManagementAPI.Models;
using TaskManagementAPI.Models.DTOs;

namespace TaskManagementAPI.Validators;

public class UpdateTaskItemDtoValidator : AbstractValidator<UpdateTaskItemDto>
{
    public UpdateTaskItemDtoValidator()
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

        // Business rule: Cannot mark task as incomplete if it was completed more than 7 days ago
        RuleFor(x => x.IsCompleted)
            .Must(AllowStatusChange)
            .When(x => x.IsCompleted == false)
            .WithMessage("Cannot mark task as incomplete - this validation would require additional context");

        // Business rule: High/Critical tasks should have a due date
        RuleFor(x => x.DueDate)
            .NotNull()
            .When(x => x.Priority == TaskPriority.High || x.Priority == TaskPriority.Critical)
            .WithMessage("High and Critical priority tasks should have a due date");

        // Cross-property validation example 
        RuleFor(x => x)
            .Must(BeValidTaskUpdate)
            .WithMessage("Invalid task update combination")
            .WithName("TaskUpdate");
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

    private static bool AllowStatusChange(bool isCompleted)
    {
        // In a real application, you might check against database
        // For demo purposes, we'll always allow the change
        return true;
    }

    private static bool BeValidTaskUpdate(UpdateTaskItemDto dto)
    {
        // Example: If marking as completed, ensure it has a proper title and description
        if (dto.IsCompleted)
        {
            return !string.IsNullOrWhiteSpace(dto.Title) && 
                   (dto.Priority != TaskPriority.Critical || !string.IsNullOrWhiteSpace(dto.Description));
        }

        return true;
    }
}