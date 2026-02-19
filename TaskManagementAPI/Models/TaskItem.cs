using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models;

public class TaskItem
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? DueDate { get; set; }
}

public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}