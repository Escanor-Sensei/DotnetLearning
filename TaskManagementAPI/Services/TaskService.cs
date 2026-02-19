using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using TaskManagementAPI.Models.DTOs;

namespace TaskManagementAPI.Services;

public class TaskService : ITaskService
{
    private readonly TaskDbContext _context;

    public TaskService(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItemDto>> GetAllTasksAsync()
    {
        var tasks = await _context.Tasks
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<TaskItemDto?> GetTaskByIdAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        return task != null ? MapToDto(task) : null;
    }

    public async Task<TaskItemDto> CreateTaskAsync(CreateTaskItemDto createTaskDto)
    {
        var task = new TaskItem
        {
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            Priority = createTaskDto.Priority,
            DueDate = createTaskDto.DueDate,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<TaskItemDto?> UpdateTaskAsync(int id, UpdateTaskItemDto updateTaskDto)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return null;

        task.Title = updateTaskDto.Title;
        task.Description = updateTaskDto.Description;
        task.IsCompleted = updateTaskDto.IsCompleted;
        task.Priority = updateTaskDto.Priority;
        task.DueDate = updateTaskDto.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TaskItemDto>> GetTasksByStatusAsync(bool isCompleted)
    {
        var tasks = await _context.Tasks
            .Where(t => t.IsCompleted == isCompleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<IEnumerable<TaskItemDto>> GetTasksByPriorityAsync(TaskPriority priority)
    {
        var tasks = await _context.Tasks
            .Where(t => t.Priority == priority)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    private static TaskItemDto MapToDto(TaskItem task)
    {
        return new TaskItemDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            Priority = task.Priority,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            DueDate = task.DueDate
        };
    }
}