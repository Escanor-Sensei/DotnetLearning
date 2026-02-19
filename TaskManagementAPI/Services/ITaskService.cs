using TaskManagementAPI.Models;
using TaskManagementAPI.Models.DTOs;

namespace TaskManagementAPI.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskItemDto>> GetAllTasksAsync();
    Task<TaskItemDto?> GetTaskByIdAsync(int id);
    Task<TaskItemDto> CreateTaskAsync(CreateTaskItemDto createTaskDto);
    Task<TaskItemDto?> UpdateTaskAsync(int id, UpdateTaskItemDto updateTaskDto);
    Task<bool> DeleteTaskAsync(int id);
    Task<IEnumerable<TaskItemDto>> GetTasksByStatusAsync(bool isCompleted);
    Task<IEnumerable<TaskItemDto>> GetTasksByPriorityAsync(TaskPriority priority);
}