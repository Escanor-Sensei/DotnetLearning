using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models.DTOs;
using TaskManagementAPI.Services;

namespace TaskManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TaskDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ITelemetryService _telemetryService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(TaskDbContext context, IJwtService jwtService, ITelemetryService telemetryService, ILogger<AuthController> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _telemetryService = telemetryService;
        _logger = logger;
    }

    /// <summary>
    /// User login
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Find user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

        if (user == null)
        {
            _logger.LogWarning("Login attempt with invalid username: {Username}", loginDto.Username);
            
            // Track failed login attempt
            _telemetryService.TrackUserLogin(loginDto.Username, "Unknown", false);
            
            return Unauthorized(new { message = "Invalid username or password" });
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for user: {Username}", loginDto.Username);
            
            // Track failed login attempt
            _telemetryService.TrackUserLogin(user.Username, user.Role, false);
            
            return Unauthorized(new { message = "Invalid username or password" });
        }

        // Generate token
        var token = _jwtService.GenerateToken(user);
        var expiration = DateTime.UtcNow.AddMinutes(60);

        // Track successful login
        _telemetryService.TrackUserLogin(user.Username, user.Role, true);
        
        _logger.LogInformation("Successful login for user: {Username}", user.Username);

        return Ok(new LoginResponseDto
        {
            Token = token,
            Expiration = expiration,
            Username = user.Username,
            Role = user.Role
        });
    }

    /// <summary>
    /// Get test user credentials
    /// </summary>
    [HttpGet("test-users")]
    public IActionResult GetTestUsers()
    {
        return Ok(new
        {
            admin = new { username = "admin", password = "Admin123!", role = "Admin" },
            user = new { username = "user", password = "User123!", role = "User" }
        });
    }
}