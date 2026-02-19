using TaskManagementAPI.Models;

namespace TaskManagementAPI.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}