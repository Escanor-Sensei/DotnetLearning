using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagementAPI.Controllers;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using TaskManagementAPI.Models.DTOs;
using TaskManagementAPI.Services;

namespace TaskManagementAPI.Tests.Controllers;

public class AuthControllerTests : IDisposable
{
    private readonly TaskDbContext _context;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ITelemetryService> _mockTelemetryService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new TaskDbContext(options);
        _mockJwtService = new Mock<IJwtService>();
        _mockTelemetryService = new Mock<ITelemetryService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        
        _controller = new AuthController(
            _context,
            _mockJwtService.Object,
            _mockTelemetryService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        const string username = "testuser";
        const string password = "password123";
        const string expectedToken = "test-jwt-token";

        // Add a user to the in-memory database
        var user = new User
        {
            Username = username,
            PasswordHash = "hashed_" + password, // Simplified hash for testing
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Username = username,
            Password = password
        };

        _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<User>()))
                      .Returns(expectedToken);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<object>(okResult.Value);
        
        // Use reflection to check the anonymous object properties
        var tokenProperty = response.GetType().GetProperty("token");
        Assert.NotNull(tokenProperty);
        Assert.Equal(expectedToken, tokenProperty.GetValue(response));

        _mockJwtService.Verify(x => x.GenerateToken(It.Is<User>(u => u.Username == username)), Times.Once);
    }

    [Fact]
    public async Task Login_InvalidUsername_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Username = "nonexistentuser",
            Password = "password123"
        };

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
        _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        const string username = "testuser";
        const string correctPassword = "correctpassword";
        const string wrongPassword = "wrongpassword";

        // Add a user with correct password
        var user = new User
        {
            Username = username,
            PasswordHash = "hashed_" + correctPassword, // Simplified hash for testing
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Username = username,
            Password = wrongPassword
        };

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
        _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_EmptyCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Username = "",
            Password = ""
        };

        // Add model validation error manually (simulating model validation)
        _controller.ModelState.AddModelError("Username", "Username is required");
        _controller.ModelState.AddModelError("Password", "Password is required");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact] 
    public async Task Login_ValidEmailAsUsername_ReturnsOkWithToken()
    {
        // Arrange
        const string email = "test@example.com";
        const string password = "password123";
        const string expectedToken = "test-jwt-token";

        var user = new User
        {
            Username = "testuser",
            Role = "User",
            PasswordHash = "hashed_" + password,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Username = email, // Using email as username
            Password = password
        };

        _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<User>()))
                      .Returns(expectedToken);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<object>(okResult.Value);
        
        var tokenProperty = response.GetType().GetProperty("token");
        Assert.NotNull(tokenProperty);
        Assert.Equal(expectedToken, tokenProperty.GetValue(response));

        _mockJwtService.Verify(x => x.GenerateToken(It.Is<User>(u => u.Username == email)), Times.Once);
    }

    [Fact]
    public async Task Login_TelemetryServiceCalled_OnSuccessfulLogin()
    {
        // Arrange
        const string username = "testuser";
        const string password = "password123";
        const string expectedToken = "test-jwt-token";

        var user = new User
        {
            Username = username,
            PasswordHash = "hashed_" + password, // Simplified hash for testing
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Username = username,
            Password = password
        };

        _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<User>()))
                      .Returns(expectedToken);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        
        // Verify telemetry service was called for login
        _mockTelemetryService.Verify(
            x => x.TrackUserLogin(It.Is<string>(s => s == username), 
                                  It.IsAny<string>(),
                                  It.Is<bool>(b => b == true)),
            Times.Once);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}