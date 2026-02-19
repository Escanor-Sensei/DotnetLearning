using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementAPI.Data;
using TaskManagementAPI.Tests.Builders;

namespace TaskManagementAPI.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration testing with in-memory database
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the app's DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TaskDbContext>));
            
            if (descriptor != null)
                services.Remove(descriptor);

            // Add InMemory database for testing
            services.AddDbContext<TaskDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();

            // Ensure the database is created and seed test data
            db.Database.EnsureCreated();
            SeedTestData(db);
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Seeds the test database with initial data
    /// </summary>
    private static void SeedTestData(TaskDbContext context)
    {
        try
        {
            // Clear existing data
            context.Tasks.RemoveRange(context.Tasks);
            context.Users.RemoveRange(context.Users);
            context.SaveChanges();

            // Add test users
            context.Users.AddRange(
                TestData.Users.Admin,
                TestData.Users.RegularUser
            );

            // Add test tasks
            context.Tasks.AddRange(
                TestData.Tasks.Simple,
                TestData.Tasks.Critical,
                TestData.Tasks.Completed,
                TestData.Tasks.Overdue
            );

            context.SaveChanges();
        }
        catch (Exception ex)
        {
            // Log or handle seeding errors
            System.Diagnostics.Debug.WriteLine($"Error seeding test data: {ex.Message}");
        }
    }
}