using FluentValidation;
using TaskManagementAPI.Models.DTOs;
using System.Text.RegularExpressions;

namespace TaskManagementAPI.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex UsernameRegex = new(
        @"^[a-zA-Z0-9_-]{3,20}$",
        RegexOptions.Compiled);

    public LoginDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
            .Must(BeValidUsernameOrEmail).WithMessage("Username must be a valid username (3-20 alphanumeric characters, underscores, or hyphens) or a valid email address");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long");
            // Additional password complexity rules could be added here
    }

    private static bool BeValidUsernameOrEmail(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        // Check if it's a valid email
        if (EmailRegex.IsMatch(username))
            return true;

        // Check if it's a valid username (alphanumeric, underscore, hyphen, 3-20 chars)
        if (UsernameRegex.IsMatch(username))
            return true;

        return false;
    }
}