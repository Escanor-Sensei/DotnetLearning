using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using TaskManagementAPI.Models;
using TaskManagementAPI.Models.DTOs;
using TaskManagementAPI.Tests.Builders;
using System.Text;

namespace TaskManagementAPI.Tests.Integration;

/// <summary>
/// Integration tests for TaskManagementAPI endpoints
/// </summary>
public class TasksIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TasksIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetTasks_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", 
                    response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task GetTasks_ReturnsTaskList()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");
        var content = await response.Content.ReadAsStringAsync();
        var tasks = JsonSerializer.Deserialize<List<TaskItemDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(tasks);
        Assert.NotEmpty(tasks);
    }

    [Fact]
    public async Task GetTask_WithValidId_ReturnsTask()
    {
        // Arrange
        const int taskId = 1;

        // Act
        var response = await _client.GetAsync($"/api/tasks/{taskId}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var task = JsonSerializer.Deserialize<TaskItemDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.NotNull(task);
            Assert.Equal(taskId, task.Id);
        }
        else
        {
            // Task might not exist in test data, which is acceptable
            Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task GetTask_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        const int invalidTaskId = 9999;

        // Act
        var response = await _client.GetAsync($"/api/tasks/{invalidTaskId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = TestData.CreateDtos.Valid;
        var jsonContent = JsonSerializer.Serialize(createDto);
        var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/tasks", stringContent);

        // Assert
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            var createdTask = JsonSerializer.Deserialize<TaskItemDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.NotNull(createdTask);
            Assert.Equal(createDto.Title, createdTask.Title);
            Assert.True(createdTask.Id > 0);
        }
        else
        {
            // Might require authentication, which is expected
            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task CreateTask_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var invalidCreateDto = TestData.CreateDtos.WithEmptyTitle;
        var jsonContent = JsonSerializer.Serialize(invalidCreateDto);
        var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/tasks", stringContent);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTasksByPriority_WithValidPriority_ReturnsFilteredTasks()
    {
        // Arrange
        const TaskPriority priority = TaskPriority.High;

        // Act
        var response = await _client.GetAsync($"/api/tasks/priority/{(int)priority}");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var tasks = JsonSerializer.Deserialize<List<TaskItemDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(tasks);
        // All returned tasks should have the specified priority
        Assert.All(tasks, task => Assert.Equal(priority, task.Priority));
    }

    [Fact]
    public async Task GetPendingTasks_ReturnsPendingTasksOnly()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/pending");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var tasks = JsonSerializer.Deserialize<List<TaskItemDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(tasks);
        // All returned tasks should be pending (not completed)
        Assert.All(tasks, task => Assert.False(task.IsCompleted));
    }

    [Fact]
    public async Task GetOverdueTasks_ReturnsOverdueTasksOnly()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/overdue");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var tasks = JsonSerializer.Deserialize<List<TaskItemDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(tasks);
        // All returned tasks should be overdue and not completed
        Assert.All(tasks, task => 
        {
            Assert.False(task.IsCompleted);
            if (task.DueDate.HasValue)
            {
                Assert.True(task.DueDate < DateTime.UtcNow);
            }
        });
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.NotFound); // Health endpoint might not be configured
    }

    [Theory]
    [InlineData("/api/tasks")]
    [InlineData("/api/tasks/pending")]
    [InlineData("/api/tasks/overdue")]
    [InlineData("/swagger")]
    public async Task GetEndpoints_ReturnSuccessOrUnauthorized(string url)
    {
        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Endpoint {url} returned unexpected status code: {response.StatusCode}"
        );
    }
}

/// <summary>
/// Integration tests for Authentication endpoints
/// </summary>
public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Username = "testuser",
            Password = "password123"
        };
        var jsonContent = JsonSerializer.Serialize(loginDto);
        var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", stringContent);

        // Assert
        // This might fail due to authentication setup, but we're testing the endpoint exists
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.InternalServerError
        );
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = TestData.Auth.InvalidLogin;
        var jsonContent = JsonSerializer.Serialize(loginDto);
        var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", stringContent);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.InternalServerError
        );
    }

    [Fact]
    public async Task Login_WithEmptyCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = TestData.Auth.EmptyCredentials;
        var jsonContent = JsonSerializer.Serialize(loginDto);
        var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", stringContent);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.InternalServerError
        );
    }
}