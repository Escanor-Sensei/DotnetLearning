using TaskManagementAPI.Models;

namespace TaskManagementAPI.Data;

public static class DataSeed
{
    public static void Initialize(TaskDbContext context)
    {
        // Check if data already exists
        if (context.Users.Any())
        {
            return; // Database has been seeded
        }

        // Create test users
        var users = new List<User>
        {
            new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", BCrypt.Net.BCrypt.GenerateSalt()),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new User
            {
                Username = "user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!", BCrypt.Net.BCrypt.GenerateSalt()),
                Role = "User", 
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        context.Users.AddRange(users);
        context.SaveChanges();

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Complete project setup",
                Description = "Set up the basic structure for the task management API",
                Priority = TaskPriority.High,
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new TaskItem
            {
                Title = "Implement CRUD operations",
                Description = "Add Create, Read, Update, and Delete functionality",
                Priority = TaskPriority.Critical,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                DueDate = DateTime.UtcNow.AddDays(2)
            },
            new TaskItem
            {
                Title = "Add unit tests",
                Description = "Write comprehensive unit tests for all API endpoints",
                Priority = TaskPriority.Medium,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(7)
            },
            new TaskItem
            {
                Title = "Document API endpoints",
                Description = "Create detailed documentation for all available endpoints",
                Priority = TaskPriority.Low,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                DueDate = DateTime.UtcNow.AddDays(10)
            }
        };

        context.Tasks.AddRange(tasks);
        context.SaveChanges();
    }
}