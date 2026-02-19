# Dapper - High Performance Micro-ORM

## Overview
Dapper is a simple object mapper for .NET and owns the title of "King of Micro ORM" in terms of speed and efficiency. It extends the IDbConnection interface, providing many useful methods for executing SQL queries.

## Key Features
- **High Performance**: Fastest ORM available for .NET
- **Lightweight**: Minimal overhead, single file
- **SQL Control**: Write your own SQL queries
- **Flexible Mapping**: Works with any database that has a .NET connector
- **No Configuration**: No XML files, no attributes required
- **Dynamic Support**: Works with dynamic objects
- **Async Support**: Full async/await support

## When to Use Dapper
✅ **Good For:**
- High-performance applications
- Complex queries requiring fine-tuned SQL
- Legacy database schemas
- Microservices with simple data access
- When you want full control over SQL
- Database-first development approach

❌ **Not Ideal For:**
- Complex object graphs with deep relationships
- Rapid prototyping with changing schemas
- Heavy ORM features (change tracking, lazy loading)
- Code-first development

## Core Concepts

### 1. Connection Management
```csharp
// SQL Server example
string connectionString = "Server=localhost;Database=TaskDB;Trusted_Connection=true;";
using var connection = new SqlConnection(connectionString);
```

### 2. Query Methods
- **Query<T>()** - Return IEnumerable<T>
- **QuerySingle<T>()** - Return single T
- **QueryFirst<T>()** - Return first T or exception
- **QueryFirstOrDefault<T>()** - Return first T or default
- **Execute()** - Execute command, return affected rows
- **ExecuteScalar<T>()** - Return single value

### 3. Parameter Binding
```csharp
// Anonymous parameters
var user = connection.QuerySingle<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = 1 });

// DynamicParameters
var parameters = new DynamicParameters();
parameters.Add("@Id", 1);
var user = connection.QuerySingle<User>("SELECT * FROM Users WHERE Id = @Id", parameters);
```

## Implementation Patterns

### 1. Repository Pattern
```csharp
public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllAsync();
    Task<TaskItem> GetByIdAsync(int id);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<bool> UpdateAsync(TaskItem task);
    Task<bool> DeleteAsync(int id);
}

public class TaskRepository : ITaskRepository
{
    private readonly IDbConnection _connection;
    
    public TaskRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    
    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Tasks ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<TaskItem>(sql);
    }
}
```

### 2. Unit of Work Pattern
```csharp
public interface IUnitOfWork : IDisposable
{
    ITaskRepository Tasks { get; }
    IUserRepository Users { get; }
    Task<int> CommitAsync();
    Task BeginTransactionAsync();
    Task RollbackAsync();
}
```

### 3. Connection Factory Pattern
```csharp
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    
    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
```

## Advanced Features

### 1. Multi-Mapping
```csharp
// Join queries with object mapping
const string sql = @"
    SELECT t.*, u.* 
    FROM Tasks t 
    INNER JOIN Users u ON t.CreatedBy = u.Id 
    WHERE t.Id = @TaskId";

var result = await connection.QueryAsync<TaskItem, User, TaskItem>(
    sql,
    (task, user) => {
        task.CreatedByUser = user;
        return task;
    },
    new { TaskId = 1 },
    splitOn: "Id");
```

### 2. Multiple Result Sets
```csharp
const string sql = @"
    SELECT * FROM Tasks WHERE CreatedBy = @UserId;
    SELECT * FROM Users WHERE Id = @UserId;";

using var multi = await connection.QueryMultipleAsync(sql, new { UserId = 1 });
var tasks = await multi.ReadAsync<TaskItem>();
var user = await multi.ReadSingleAsync<User>();
```

### 3. Stored Procedures
```csharp
var parameters = new DynamicParameters();
parameters.Add("@Title", "Task Title");
parameters.Add("@Description", "Task Description");
parameters.Add("@TaskId", dbType: DbType.Int32, direction: ParameterDirection.Output);

await connection.ExecuteAsync("sp_CreateTask", parameters, commandType: CommandType.StoredProcedure);
var taskId = parameters.Get<int>("@TaskId");
```

### 4. Bulk Operations
```csharp
// Bulk insert
const string sql = "INSERT INTO Tasks (Title, Description, Priority) VALUES (@Title, @Description, @Priority)";
await connection.ExecuteAsync(sql, taskList);

// Using SqlBulkCopy for large datasets
public async Task BulkInsertAsync<T>(IEnumerable<T> entities, string tableName)
{
    var dataTable = ToDataTable(entities);
    using var bulkCopy = new SqlBulkCopy(_connectionString);
    bulkCopy.DestinationTableName = tableName;
    await bulkCopy.WriteToServerAsync(dataTable);
}
```

## Performance Optimization

### 1. Connection Pooling
- Use connection pooling (enabled by default in most providers)
- Don't hold connections open longer than necessary
- Use `using` statements for proper disposal

### 2. Compiled Queries
```csharp
// For frequently executed queries, consider caching compiled expressions
private static readonly ConcurrentDictionary<string, Func<IDbConnection, object, Task<IEnumerable<T>>>> 
    CompiledQueries = new();
```

### 3. Query Optimization
- Use parameterized queries (prevents SQL injection)
- Select only required columns
- Use appropriate indexes
- Consider query plans and execution statistics

## Error Handling

### 1. Custom Exceptions
```csharp
public class DataAccessException : Exception
{
    public string SqlQuery { get; }
    public object Parameters { get; }
    
    public DataAccessException(string message, string sqlQuery, object parameters, Exception innerException)
        : base(message, innerException)
    {
        SqlQuery = sqlQuery;
        Parameters = parameters;
    }
}
```

### 2. Retry Policies
```csharp
public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await operation();
        }
        catch (SqlException ex) when (i < maxRetries - 1 && IsTransientError(ex))
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // Exponential backoff
        }
    }
    
    return await operation(); // Final attempt without catching
}
```

## Testing Strategies

### 1. Integration Testing
```csharp
public class TaskRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly ITaskRepository _repository;
    
    [Fact]
    public async Task CreateAsync_ValidTask_ReturnsTaskWithId()
    {
        // Arrange
        var task = new TaskItem { Title = "Test Task" };
        
        // Act
        var result = await _repository.CreateAsync(task);
        
        // Assert
        Assert.True(result.Id > 0);
    }
}
```

### 2. Mocking with Dapper
```csharp
// Using MockDbConnection for unit tests
public class MockTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = new();
    
    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        task.Id = _tasks.Count + 1;
        _tasks.Add(task);
        return task;
    }
}
```

## Best Practices

### 1. Organization
- Keep SQL in repository classes
- Use constants for frequently used queries
- Consider SQL files for complex queries
- Organize repositories by aggregate root

### 2. Security
- Always use parameterized queries
- Validate input parameters
- Implement proper authorization
- Use least privilege database accounts

### 3. Maintainability
- Use meaningful parameter names
- Document complex queries
- Follow consistent naming conventions
- Implement proper logging

### 4. Configuration
```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TaskDB;Trusted_Connection=true;TrustServerCertificate=true;",
    "ReadOnlyConnection": "Server=readonly-server;Database=TaskDB;Trusted_Connection=true;"
  },
  "DatabaseOptions": {
    "CommandTimeout": 30,
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 3
  }
}
```

## Migration from Entity Framework

### 1. Mapping Comparison
| EF Core | Dapper |
|---------|--------|
| DbContext | IDbConnection |
| DbSet<T> | Repository<T> |
| Include() | Multiple queries or joins |
| SaveChanges() | Manual Execute() calls |
| Change Tracking | Manual state management |
| Migrations | Manual SQL scripts |

### 2. Benefits of Migration
- **Performance**: Significantly faster queries
- **Control**: Full SQL control and optimization
- **Simplicity**: Less abstraction, easier debugging
- **Memory**: Lower memory footprint
- **Database Features**: Direct access to database-specific features

### 3. Challenges
- **More Code**: Manual mapping and queries
- **No Change Tracking**: Manual state management
- **Relationships**: Manual handling of complex objects
- **Schema Changes**: Manual migration scripts

## Common Patterns

### 1. Generic Repository
```csharp
public abstract class BaseRepository<T> where T : class
{
    protected readonly IDbConnection _connection;
    protected abstract string TableName { get; }
    
    protected BaseRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _connection.QueryAsync<T>($"SELECT * FROM {TableName}");
    }
}
```

### 2. Specification Pattern
```csharp
public interface ISpecification<T>
{
    string ToSqlWhere();
    object GetParameters();
}

public class TasksByPrioritySpec : ISpecification<TaskItem>
{
    private readonly TaskPriority _priority;
    
    public string ToSqlWhere() => "Priority = @Priority";
    public object GetParameters() => new { Priority = _priority };
}
```

## Tools and Extensions

### 1. Useful NuGet Packages
- **Dapper** - Core package
- **Dapper.Contrib** - Simple CRUD extensions
- **Dapper.SqlBuilder** - Dynamic SQL building
- **Dapper.Transaction** - Transaction helpers
- **Microsoft.Data.SqlClient** - SQL Server provider

### 2. Development Tools
- **SQL Server Management Studio** - Query development
- **Azure Data Studio** - Cross-platform SQL tool
- **Dapper Extensions** - Additional helper methods
- **MiniProfiler.Dapper** - Performance profiling

### 3. SQL Building Helpers
```csharp
// Using SqlBuilder for dynamic queries
var builder = new SqlBuilder();
var template = builder.AddTemplate("SELECT * FROM Tasks /**where**/");

if (priority.HasValue)
    builder.Where("Priority = @Priority", new { Priority = priority });

if (!includeCompleted)
    builder.Where("IsCompleted = 0");

var results = await connection.QueryAsync<TaskItem>(template.RawSql, template.Parameters);
```

## Database Schema Considerations

### 1. Table Design
- Use appropriate data types
- Implement proper constraints
- Create necessary indexes
- Consider partitioning for large tables

### 2. Stored Procedures
- Use for complex business logic
- Better performance for complex operations
- Easier to maintain business rules
- Better security isolation

### 3. Views and Functions
- Create views for complex joins
- Use functions for calculated fields
- Implement for reporting queries
- Consider indexed views for performance

## Deployment and Migration

### 1. Database Versioning
- Use migration scripts in version control
- Implement database versioning table
- Create rollback scripts
- Test migrations in staging environment

### 2. Connection String Management
- Use configuration providers
- Implement environment-specific settings
- Secure sensitive connection information
- Use Azure Key Vault or similar for production

This comprehensive guide covers all aspects of using Dapper effectively in .NET applications, from basic concepts to advanced patterns and best practices.