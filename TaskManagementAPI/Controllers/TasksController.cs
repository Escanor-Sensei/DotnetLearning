using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.Models;
using TaskManagementAPI.Models.DTOs;
using TaskManagementAPI.Services;

namespace TaskManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// Get all tasks
    /// </summary>
    /// <returns>List of all tasks</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetAllTasks()
    {
        _logger.LogInformation("Getting all tasks");
        var tasks = await _taskService.GetAllTasksAsync();
        return Ok(tasks);
    }

    /// <summary>
    /// Get a specific task by ID
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Task details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemDto>> GetTask(int id)
    {
        _logger.LogInformation("Getting task with ID: {TaskId}", id);
        
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found", id);
            return NotFound($"Task with ID {id} not found");
        }

        return Ok(task);
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    /// <param name="createTaskDto">Task creation details</param>
    /// <returns>Created task</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskItemDto>> CreateTask([FromBody] CreateTaskItemDto createTaskDto)
    {
        _logger.LogInformation("Creating new task: {TaskTitle}", createTaskDto.Title);
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdTask = await _taskService.CreateTaskAsync(createTaskDto);
        
        _logger.LogInformation("Task created with ID: {TaskId}", createdTask.Id);
        return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    /// <param name="id">Task ID to update</param>
    /// <param name="updateTaskDto">Updated task details</param>
    /// <returns>Updated task</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemDto>> UpdateTask(int id, [FromBody] UpdateTaskItemDto updateTaskDto)
    {
        _logger.LogInformation("Updating task with ID: {TaskId}", id);
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedTask = await _taskService.UpdateTaskAsync(id, updateTaskDto);
        if (updatedTask == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found for update", id);
            return NotFound($"Task with ID {id} not found");
        }

        _logger.LogInformation("Task with ID {TaskId} updated successfully", id);
        return Ok(updatedTask);
    }

    /// <summary>
    /// Delete a task (Admin only)
    /// </summary>
    /// <param name="id">Task ID to delete</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin can delete tasks
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        _logger.LogInformation("Deleting task with ID: {TaskId}", id);
        
        var deleted = await _taskService.DeleteTaskAsync(id);
        if (!deleted)
        {
            _logger.LogWarning("Task with ID {TaskId} not found for deletion", id);
            return NotFound($"Task with ID {id} not found");
        }

        _logger.LogInformation("Task with ID {TaskId} deleted successfully", id);
        return NoContent();
    }

    /// <summary>
    /// Get tasks filtered by completion status
    /// </summary>
    /// <param name="completed">Completion status filter</param>
    /// <returns>Filtered list of tasks</returns>
    [HttpGet("filter/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasksByStatus([FromQuery] bool completed)
    {
        _logger.LogInformation("Getting tasks with completion status: {IsCompleted}", completed);
        var tasks = await _taskService.GetTasksByStatusAsync(completed);
        return Ok(tasks);
    }

    /// <summary>
    /// Get tasks filtered by priority
    /// </summary>
    /// <param name="priority">Priority level filter</param>
    /// <returns>Filtered list of tasks</returns>
    [HttpGet("filter/priority/{priority}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasksByPriority(TaskPriority priority)
    {
        _logger.LogInformation("Getting tasks with priority: {Priority}", priority);
        var tasks = await _taskService.GetTasksByPriorityAsync(priority);
        return Ok(tasks);
    }
}