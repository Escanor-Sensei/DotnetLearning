# Simple JWT Authentication & Role-Based Authorization

## 🔐 **Overview**

This guide covers implementing minimal JWT authentication and role-based authorization in .NET 8 Web API with the least possible changes to existing code.

## 🏗️ **Implementation Strategy**

### **1. Minimal Dependencies**
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT token validation
- `System.IdentityModel.Tokens.Jwt` - JWT token creation
- `BCrypt.Net-Next` - Password hashing

### **2. Core Components**
- **Simple User Model** - Just ID, Username, Password, Role
- **JWT Service** - Token generation and validation
- **Auth Controller** - Login endpoint
- **Authorization Attributes** - Role-based protection

## 💡 **Key Concepts**

### **JWT Claims**
Claims are key-value pairs embedded in JWT tokens:
```json
{
  "sub": "123",           // User ID
  "unique_name": "john",  // Username  
  "role": "User",         // User Role
  "exp": 1645123456       // Expiration
}
```

### **Role-Based Authorization**
```csharp
[Authorize]                    // Any authenticated user
[Authorize(Roles = "Admin")]   // Admin users only
[Authorize(Roles = "User,Admin")] // User OR Admin
```

## 🚀 **Implementation Steps**

### **Step 1: Add JWT Dependencies**
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.1.2" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

### **Step 2: JWT Configuration**
```json
// appsettings.json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-minimum-256-bits-long",
    "Issuer": "TaskManagementAPI",
    "Audience": "TaskManagementAPI-Users",
    "ExpirationMinutes": 60
  }
}
```

### **Step 3: Simple User Model**
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } = "User"; // "User" or "Admin"
}
```

### **Step 4: JWT Service**
```csharp
public interface IJwtService
{
    string GenerateToken(User user);
}

public class JwtService : IJwtService
{
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };
        
        // Create and return JWT token
    }
}
```

### **Step 5: Auth Controller**
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        // Validate user credentials
        // Generate JWT token
        // Return token
    }
}
```

### **Step 6: Configure Authentication Pipeline**
```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        // Configure JWT validation
    });

// Middleware order is critical!
app.UseAuthentication(); // First - Who you are
app.UseAuthorization();  // Second - What you can do
```

### **Step 7: Protect Endpoints**
```csharp
[Authorize] // Requires authentication
public class TasksController : ControllerBase
{
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only admins can delete
    public async Task<IActionResult> DeleteTask(int id) { }
}
```

## 🧪 **Testing Authentication**

### **1. Login Request**
```http
POST /api/auth/login
{
    "username": "admin",
    "password": "Admin123!"
}
```

### **2. Response**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiration": "2026-02-19T12:00:00Z"
}
```

### **3. Using Token**
```http
GET /api/tasks
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 📋 **Default Test Users**
- **Username:** `admin` | **Password:** `Admin123!` | **Role:** Admin
- **Username:** `user` | **Password:** `User123!` | **Role:** User

## 🔧 **Key HTTP Status Codes**
- `200 OK` - Successful login
- `401 Unauthorized` - Invalid credentials or no token
- `403 Forbidden` - Valid token but insufficient role permissions

## 🎯 **Learning Benefits**

1. **Stateless Authentication** - No server-side session storage
2. **Claims-Based Identity** - Flexible user information in tokens
3. **Role Separation** - Clear admin vs user permissions
4. **Industry Standard** - JWT is widely used for APIs
5. **Easy Integration** - Works with Swagger UI for testing

## 🚨 **Security Notes**

- **Secret Key**: Must be 256+ bits for production
- **HTTPS Only**: Never send JWT over HTTP
- **Token Expiration**: Keep it short (15-60 minutes)
- **Password Hashing**: Always use BCrypt or similar
- **Input Validation**: Validate all login inputs

---

*This implementation provides enterprise-grade authentication with minimal code changes to your existing API.*