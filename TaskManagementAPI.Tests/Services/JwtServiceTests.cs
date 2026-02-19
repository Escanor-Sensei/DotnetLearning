using Microsoft.Extensions.Configuration;
using Moq;
using TaskManagementAPI.Models;
using TaskManagementAPI.Services;
using TaskManagementAPI.Tests.Builders;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TaskManagementAPI.Tests.Services;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly JwtService _jwtService;
    private const string TestSecretKey = "MyVeryLongSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm";

    public JwtServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        
        // Setup JWT configuration
        var mockJwtSection = new Mock<IConfigurationSection>();
        mockJwtSection.Setup(x => x["SecretKey"]).Returns(TestSecretKey);
        mockJwtSection.Setup(x => x["Issuer"]).Returns("TestIssuer");
        mockJwtSection.Setup(x => x["Audience"]).Returns("TestAudience");
        mockJwtSection.Setup(x => x["ExpirationMinutes"]).Returns("60");

        _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(mockJwtSection.Object);
        
        _jwtService = new JwtService(_mockConfiguration.Object);
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsValidJwtToken()
    {
        // Arrange
        var user = new UserBuilder()
            .WithId(1)
            .WithUsername("testuser")
            .WithRole("User")
            .Build();

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Verify token format
        var tokenHandler = new JwtSecurityTokenHandler();
        Assert.True(tokenHandler.CanReadToken(token));
        
        var jsonToken = tokenHandler.ReadJwtToken(token);
        Assert.Equal("TestIssuer", jsonToken.Issuer);
        Assert.Contains(jsonToken.Audiences, a => a == "TestAudience");
    }

    [Fact]
    public void GenerateToken_ValidUser_ContainsExpectedClaims()
    {
        // Arrange
        var user = new UserBuilder()
            .WithId(123)
            .WithUsername("johndoe")
            .WithRole("Admin")
            .Build();

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        // Check required claims
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == "123");
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.Name && c.Value == "johndoe");
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateToken_ValidUser_TokenHasCorrectExpiration()
    {
        // Arrange
        var user = TestData.Users.RegularUser;
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _jwtService.GenerateToken(user);
        var afterGeneration = DateTime.UtcNow;

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        // Token should expire in approximately 60 minutes (as configured)
        var expectedMinExpiration = beforeGeneration.AddMinutes(59);
        var expectedMaxExpiration = afterGeneration.AddMinutes(61);
        
        Assert.True(jsonToken.ValidTo >= expectedMinExpiration);
        Assert.True(jsonToken.ValidTo <= expectedMaxExpiration);
    }

    [Fact]
    public void GenerateToken_AdminUser_ContainsAdminRole()
    {
        // Arrange
        var adminUser = new UserBuilder()
            .WithUsername("admin")
            .WithRole("Admin")
            .Build();

        // Act
        var token = _jwtService.GenerateToken(adminUser);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateToken_RegularUser_ContainsUserRole()
    {
        // Arrange
        var regularUser = new UserBuilder()
            .WithUsername("user")
            .WithRole("User")
            .Build();

        // Act
        var token = _jwtService.GenerateToken(regularUser);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
    }

    [Fact]
    public void GenerateToken_NullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _jwtService.GenerateToken(null!));
    }

    [Fact]
    public void GenerateToken_UserWithEmptyUsername_ThrowsArgumentException()
    {
        // Arrange
        var user = new UserBuilder()
            .WithUsername("")
            .Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _jwtService.GenerateToken(user));
    }

    [Fact]
    public void GenerateToken_MultipleTokens_AreUnique()
    {
        // Arrange
        var user1 = new UserBuilder().WithId(1).WithUsername("user1").Build();
        var user2 = new UserBuilder().WithId(2).WithUsername("user2").Build();

        // Act
        var token1 = _jwtService.GenerateToken(user1);
        var token2 = _jwtService.GenerateToken(user2);

        // Assert
        Assert.NotEqual(token1, token2);
        
        // Verify tokens contain different user information
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken1 = tokenHandler.ReadJwtToken(token1);
        var jsonToken2 = tokenHandler.ReadJwtToken(token2);
        
        var userId1 = jsonToken1.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var userId2 = jsonToken2.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        
        Assert.NotEqual(userId1, userId2);
    }

    [Fact]
    public void GenerateToken_WithValidConfiguration_UsesConfiguredValues()
    {
        // Arrange
        var user = TestData.Users.RegularUser;

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        Assert.Equal("TestIssuer", jsonToken.Issuer);
        Assert.Contains("TestAudience", jsonToken.Audiences);
        
        // Verify configuration was used
        _mockConfiguration.Verify(x => x.GetSection("JwtSettings"), Times.AtLeastOnce);
    }
}