using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using TaskManagementAPI.Services;
using TaskManagementAPI.Tests.Builders;
using Microsoft.EntityFrameworkCore;

namespace TaskManagementAPI.Tests.Services;

public class TaskServiceTests : IDisposable
{
    private readonly TaskDbContext _context;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new TaskDbContext(options);
        _service = new TaskService(_context);
    }

    [Fact]
    public async Task GetAllTasksAsync_ReturnsAllTasks_Successfully()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            TestData.Tasks.Simple,
            TestData.Tasks.Critical,
            TestData.Tasks.Completed
        };

        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllTasksAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        
        var resultList = result.ToList();
        Assert.Contains(resultList, t => t.Title == "Simple Task");
        Assert.Contains(resultList, t => t.Title == "Critical Task");  
        Assert.Contains(resultList, t => t.Title == "Completed Task");
    }

    [Fact]
    public async Task GetTaskByIdAsync_ExistingTask_ReturnsTask()
    {
        // Arrange
        var task = new TaskItemBuilder()
            .WithId(1)
            .WithTitle("Test Task")
            .WithDescription("Test Description")
            .Build();

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTaskByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test Description", result.Description);
    }

    [Fact]
    public async Task GetTaskByIdAsync_NonExistentTask_ReturnsNull()
    {
        // Act
        var result = await _service.GetTaskByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateTaskAsync_ValidTask_ReturnsCreatedTask()
    {
        // Arrange
        var createDto = new CreateTaskItemDtoBuilder()
            .WithTitle("New Task")
            .WithDescription("New Description")
            .Build();

        // Act
        var result = await _service.CreateTaskAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Task", result.Title);
        Assert.Equal("New Description", result.Description);
        Assert.True(result.Id > 0);

        // Verify task was saved to database
        var savedTask = await _context.Tasks.FindAsync(result.Id);
        Assert.NotNull(savedTask);
        Assert.Equal("New Task", savedTask.Title);
    }

    [Fact]
    public async Task UpdateTaskAsync_ExistingTask_ReturnsUpdatedTask()
    {
        // Arrange
        var originalTask = new TaskItemBuilder()
            .WithId(1)
            .WithTitle("Original Title")
            .WithDescription("Original Description")
            .Build();

        _context.Tasks.Add(originalTask);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateTaskItemDtoBuilder()
            .WithTitle("Updated Title")
            .WithDescription("Updated Description")
            .Build();

        // Act
        var result = await _service.UpdateTaskAsync(1, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Description", result.Description);

        // Verify task was updated in database
        var updatedTask = await _context.Tasks.FindAsync(1);
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.Equal("Updated Description", updatedTask.Description);
    }

    [Fact]
    public async Task UpdateTaskAsync_NonExistentTask_ReturnsNull()
    {
        // Arrange
        var updateDto = new UpdateTaskItemDtoBuilder()
            .WithTitle("Updated Title")
            .Build();

        // Act
        var result = await _service.UpdateTaskAsync(999, updateDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteTaskAsync_ExistingTask_ReturnsTrue()
    {
        // Arrange
        var task = new TaskItemBuilder()
            .WithId(1)
            .WithTitle("Task to Delete")
            .Build();

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteTaskAsync(1);

        // Assert
        Assert.True(result);

        // Verify task was removed from database
        var deletedTask = await _context.Tasks.FindAsync(1);
        Assert.Null(deletedTask);
    }

    [Fact]
    public async Task DeleteTaskAsync_NonExistentTask_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteTaskAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetTasksByStatusAsync_FiltersByCompleted_Successfully()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItemBuilder().WithIsCompleted(false).WithTitle("Pending Task 1").Build(),
            new TaskItemBuilder().WithIsCompleted(false).WithTitle("Pending Task 2").Build(),
            new TaskItemBuilder().WithIsCompleted(true).WithTitle("Completed Task").Build()
        };

        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTasksByStatusAsync(false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.False(t.IsCompleted));
    }

    [Fact]
    public async Task GetTasksByPriorityAsync_FiltersByPriority_Successfully()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItemBuilder().WithPriority(TaskPriority.High).WithTitle("High Priority 1").Build(),
            new TaskItemBuilder().WithPriority(TaskPriority.High).WithTitle("High Priority 2").Build(),
            new TaskItemBuilder().WithPriority(TaskPriority.Low).WithTitle("Low Priority").Build()
        };

        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTasksByPriorityAsync(TaskPriority.High);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(TaskPriority.High, t.Priority));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}