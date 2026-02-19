using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.Models.DTOs;
using TaskManagementAPI.Validators;

namespace TaskManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValidationTestController : ControllerBase
{
    /// <summary>
    /// Test endpoint to demonstrate FluentValidation manually
    /// </summary>
    [HttpPost("test-create-task")]
    public ActionResult TestCreateTaskValidation([FromBody] CreateTaskItemDto dto)
    {
        var validator = new CreateTaskItemDtoValidator();
        var result = validator.Validate(dto);
        
        if (!result.IsValid)
        {
            return BadRequest(new 
            { 
                Message = "Manual validation failed",
                Errors = result.Errors.Select(e => new 
                { 
                    Property = e.PropertyName,
                    Message = e.ErrorMessage,
                    AttemptedValue = e.AttemptedValue
                }).ToList()
            });
        }
        
        return Ok(new 
        { 
            Message = "Manual validation passed",
            Data = dto
        });
    }

    /// <summary>
    /// Test endpoint to demonstrate automatic FluentValidation by ASP.NET Core
    /// </summary>
    [HttpPost("auto-validate-create-task")]
    public ActionResult AutoValidateCreateTask([FromBody] CreateTaskItemDto dto)
    {
        // This will automatically validate using FluentValidation
        // If validation fails, ASP.NET Core will return 400 BadRequest automatically
        return Ok(new 
        { 
            Message = "Automatic validation passed",
            Data = dto
        });
    }

    /// <summary>
    /// Test endpoint for login validation
    /// </summary>
    [HttpPost("test-login")]
    public ActionResult TestLoginValidation([FromBody] LoginDto dto)
    {
        var validator = new LoginDtoValidator();
        var result = validator.Validate(dto);
        
        if (!result.IsValid)
        {
            return BadRequest(new 
            { 
                Message = "Login validation failed",
                Errors = result.Errors.Select(e => new 
                { 
                    Property = e.PropertyName,
                    Message = e.ErrorMessage,
                    AttemptedValue = e.AttemptedValue
                }).ToList()
            });
        }
        
        return Ok(new 
        { 
            Message = "Login validation passed",
            Data = new { dto.Username } // Don't return password
        });
    }

    /// <summary>
    /// Get validation examples for testing
    /// </summary>
    [HttpGet("examples")]
    public ActionResult GetValidationExamples()
    {
        return Ok(new
        {
            ValidCreateTask = new CreateTaskItemDto
            {
                Title = "Valid Task Title",
                Description = "This is a valid task description",
                Priority = Models.TaskPriority.Medium,
                DueDate = DateTime.Now.AddDays(7)
            },
            InvalidCreateTask = new CreateTaskItemDto
            {
                Title = "", // Invalid: empty title
                Priority = Models.TaskPriority.Critical,
                Description = null, // Invalid: critical task needs description
                DueDate = DateTime.Now.AddDays(-1) // Invalid: past date
            },
            ValidLogin = new LoginDto
            {
                Username = "testuser",
                Password = "password123"
            },
            InvalidLogin = new LoginDto
            {
                Username = "", // Invalid: empty username
                Password = "123" // Invalid: too short
            }
        });
    }
}