using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Data;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure TaskItem entity
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.Property(t => t.Priority).HasConversion<int>();
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
            entity.HasIndex(u => u.Username).IsUnique();
        });
    }
}