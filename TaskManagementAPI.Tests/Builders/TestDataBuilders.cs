using TaskManagementAPI.Models;
using TaskManagementAPI.Models.DTOs;

namespace TaskManagementAPI.Tests.Builders;

/// <summary>
/// Builder for TaskItem objects used in testing
/// </summary>
public class TaskItemBuilder
{
    private int _id = 1;
    private string _title = "Test Task";
    private string? _description = "Test Description";
    private bool _isCompleted = false;
    private TaskPriority _priority = TaskPriority.Medium;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime? _updatedAt;
    private DateTime? _dueDate;

    public TaskItemBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public TaskItemBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TaskItemBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public TaskItemBuilder WithIsCompleted(bool isCompleted)
    {
        _isCompleted = isCompleted;
        return this;
    }

    public TaskItemBuilder WithPriority(TaskPriority priority)
    {
        _priority = priority;
        return this;
    }

    public TaskItemBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public TaskItemBuilder WithUpdatedAt(DateTime? updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public TaskItemBuilder WithDueDate(DateTime? dueDate)
    {
        _dueDate = dueDate;
        return this;
    }

    public TaskItem Build()
    {
        return new TaskItem
        {
            Id = _id,
            Title = _title,
            Description = _description,
            IsCompleted = _isCompleted,
            Priority = _priority,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DueDate = _dueDate
        };
    }
}

/// <summary>
/// Builder for CreateTaskItemDto objects used in testing
/// </summary>
public class CreateTaskItemDtoBuilder
{
    private string _title = "New Task";
    private string? _description = "New Description";
    private TaskPriority _priority = TaskPriority.Medium;
    private DateTime? _dueDate;

    public CreateTaskItemDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public CreateTaskItemDtoBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public CreateTaskItemDtoBuilder WithPriority(TaskPriority priority)
    {
        _priority = priority;
        return this;
    }

    public CreateTaskItemDtoBuilder WithDueDate(DateTime? dueDate)
    {
        _dueDate = dueDate;
        return this;
    }

    public CreateTaskItemDto Build()
    {
        return new CreateTaskItemDto
        {
            Title = _title,
            Description = _description,
            Priority = _priority,
            DueDate = _dueDate
        };
    }
}

/// <summary>
/// Builder for UpdateTaskItemDto objects used in testing
/// </summary>
public class UpdateTaskItemDtoBuilder
{
    private string _title = "Updated Task";
    private string? _description = "Updated Description";
    private bool _isCompleted = false;
    private TaskPriority _priority = TaskPriority.Medium;
    private DateTime? _dueDate;

    public UpdateTaskItemDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public UpdateTaskItemDtoBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public UpdateTaskItemDtoBuilder WithIsCompleted(bool isCompleted)
    {
        _isCompleted = isCompleted;
        return this;
    }

    public UpdateTaskItemDtoBuilder WithPriority(TaskPriority priority)
    {
        _priority = priority;
        return this;
    }

    public UpdateTaskItemDtoBuilder WithDueDate(DateTime? dueDate)
    {
        _dueDate = dueDate;
        return this;
    }

    public UpdateTaskItemDto Build()
    {
        return new UpdateTaskItemDto
        {
            Title = _title,
            Description = _description,
            IsCompleted = _isCompleted,
            Priority = _priority,
            DueDate = _dueDate
        };
    }
}

/// <summary>
/// Builder for User objects used in testing
/// </summary>
public class UserBuilder
{
    private int _id = 1;
    private string _username = "testuser";
    private string _passwordHash = "$2a$11$examplehash";
    private string _role = "User";
    private DateTime _createdAt = DateTime.UtcNow;
    private bool _isActive = true;

    public UserBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public UserBuilder WithRole(string role)
    {
        _role = role;
        return this;
    }

    public UserBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public UserBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public User Build()
    {
        return new User
        {
            Id = _id,
            Username = _username,
            PasswordHash = _passwordHash,
            Role = _role,
            CreatedAt = _createdAt,
            IsActive = _isActive
        };
    }
}

/// <summary>
/// Static test data for common scenarios
/// </summary>
public static class TestData
{
    public static class Tasks
    {
        public static readonly TaskItem Simple = new TaskItemBuilder()
            .WithId(1)
            .WithTitle("Simple Task")
            .WithDescription("A simple task for testing")
            .WithPriority(TaskPriority.Low)
            .Build();

        public static readonly TaskItem Critical = new TaskItemBuilder()
            .WithId(2)
            .WithTitle("Critical Task")
            .WithDescription("An urgent task")
            .WithPriority(TaskPriority.Critical)
            .WithDueDate(DateTime.Now.AddDays(1))
            .Build();

        public static readonly TaskItem Completed = new TaskItemBuilder()
            .WithId(3)
            .WithTitle("Completed Task")
            .WithDescription("This task is done")
            .WithIsCompleted(true)
            .WithUpdatedAt(DateTime.Now)
            .Build();

        public static readonly TaskItem Overdue = new TaskItemBuilder()
            .WithId(4)
            .WithTitle("Overdue Task")
            .WithDescription("This task is overdue")
            .WithPriority(TaskPriority.High)
            .WithDueDate(DateTime.Now.AddDays(-2))
            .Build();
    }

    public static class Users
    {
        public static readonly User Admin = new UserBuilder()
            .WithId(1)
            .WithUsername("admin")
            .WithRole("Admin")
            .Build();

        public static readonly User RegularUser = new UserBuilder()
            .WithId(2)
            .WithUsername("user")
            .WithRole("User")
            .Build();
    }

    public static class CreateDtos
    {
        public static readonly CreateTaskItemDto Valid = new CreateTaskItemDtoBuilder()
            .WithTitle("Valid Task")
            .WithDescription("Valid task description")
            .Build();

        public static readonly CreateTaskItemDto Standard = new CreateTaskItemDtoBuilder()
            .WithTitle("Standard Task")
            .WithDescription("Standard task description")
            .Build();

        public static readonly CreateTaskItemDto Critical = new CreateTaskItemDtoBuilder()
            .WithTitle("Critical Task")
            .WithDescription("Critical task requiring immediate attention")
            .WithPriority(TaskPriority.Critical)
            .WithDueDate(DateTime.Now.AddHours(6))
            .Build();

        public static readonly CreateTaskItemDto WithEmptyTitle = new CreateTaskItemDtoBuilder()
            .WithTitle("") // Empty title for validation testing
            .WithDescription("Task with empty title for validation")
            .Build();
    }

    public static class Auth
    {
        public static readonly LoginDto ValidLogin = new LoginDto
        {
            Username = "admin",
            Password = "admin123"
        };

        public static readonly LoginDto InvalidLogin = new LoginDto
        {
            Username = "admin",
            Password = "wrongpassword"
        };

        public static readonly LoginDto EmptyCredentials = new LoginDto
        {
            Username = "",
            Password = ""
        };
    }
}