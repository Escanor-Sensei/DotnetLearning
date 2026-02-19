# Dapper — Micro-ORM

## What is Dapper?

Dapper is a lightweight Object-Relational Mapper (ORM) for .NET that extends `IDbConnection` with convenient methods for mapping SQL query results to .NET objects. Unlike heavy ORMs (Entity Framework), Dapper gives you direct SQL control with minimal abstraction overhead.

It was created by the Stack Overflow team to solve their performance requirements and remains one of the fastest ORMs for .NET.

## Micro-ORM vs Full ORM

| Aspect | Dapper (Micro-ORM) | Entity Framework (Full ORM) |
|---|---|---|
| **SQL control** | You write all SQL | Generates SQL from LINQ |
| **Performance** | Near raw ADO.NET speed | Overhead from change tracking, lazy loading |
| **Learning curve** | Simple — just SQL + mapping | Complex — DbContext, migrations, conventions |
| **Change tracking** | None | Automatic |
| **Migrations** | Manual SQL scripts | Code-first migrations |
| **Lazy loading** | Not supported | Built-in |
| **Relationships** | Manual join mapping | Automatic navigation properties |
| **Query optimization** | You control it | Framework decides (sometimes poorly) |
| **Memory footprint** | Minimal | Higher (tracking, caching) |

## When to Choose Dapper

**Use Dapper when:**
- Performance is critical (high-throughput APIs, real-time systems)
- You need fine-tuned SQL (complex queries, database-specific features)
- You're working with legacy databases with non-standard schemas
- You want full control and transparency over data access
- The project has simple data access patterns (CRUD without complex relationships)

**Use EF Core when:**
- Rapid prototyping with frequently changing schemas
- Complex object graphs with deep relationships
- You need change tracking, lazy loading, or migrations
- Developer productivity is prioritized over raw performance
- Code-first development with evolving data models

## Core Concepts

### Connection Management

Dapper extends `IDbConnection` — it doesn't manage connections itself. You're responsible for:
- Creating connections (typically via dependency injection)
- Opening connections (Dapper opens them if closed, but best practice is explicit)
- Disposing connections (using `using` statements or DI scoped lifetime)

**Important**: Connection pooling is handled by the underlying provider (ADO.NET). Don't hold connections open longer than needed.

### Object Mapping

Dapper automatically maps query columns to object properties by matching column names to property names (case-insensitive). No configuration needed for simple cases.

| Scenario | Behavior |
|---|---|
| Column name matches property name | Automatic mapping |
| Column name differs from property | Use SQL aliases (`SELECT col AS PropertyName`) |
| Extra columns in result | Ignored |
| Missing columns in result | Property gets default value |
| Type mismatch | Throws exception at runtime |

### Parameterized Queries

Dapper supports parameterized queries to prevent SQL injection:

- **Anonymous objects** — Pass `new { Id = 1, Name = "test" }` as parameters
- **DynamicParameters** — Programmatically build parameter collections; supports output parameters and stored procedure parameters
- Parameters are automatically matched by name to `@ParameterName` in SQL

**Critical**: Never concatenate user input into SQL strings, even with Dapper.

## Query Methods

| Method | Returns | Use When |
|---|---|---|
| `QueryAsync<T>()` | `IEnumerable<T>` | Multiple rows expected |
| `QuerySingleAsync<T>()` | `T` | Exactly one row expected (throws if 0 or 2+) |
| `QuerySingleOrDefaultAsync<T>()` | `T?` | Zero or one row (throws if 2+) |
| `QueryFirstAsync<T>()` | `T` | First row (throws if 0 rows) |
| `QueryFirstOrDefaultAsync<T>()` | `T?` | First row or default (safest for optional results) |
| `ExecuteAsync()` | `int` (affected rows) | INSERT, UPDATE, DELETE, DDL |
| `ExecuteScalarAsync<T>()` | `T` | Single value (COUNT, SUM, etc.) |

### Choosing the Right Method

- Need a list? → `QueryAsync<T>`
- Need exactly one? → `QuerySingleAsync<T>` (fails fast if assumption is wrong)
- Need one or none? → `QuerySingleOrDefaultAsync<T>`
- Don't care about multiple, want first? → `QueryFirstOrDefaultAsync<T>`
- Not returning data? → `ExecuteAsync`

## Advanced Features

### Multi-Mapping (Joins)

Map a single SQL query with JOINs to multiple objects. Dapper splits the row at a specified column (`splitOn` parameter) and creates separate objects from each segment.

**How it works:**
1. Write a JOIN query that returns columns from multiple tables
2. Provide a mapping function that receives the split objects
3. Specify `splitOn` to tell Dapper where one table's columns end and the next begins
4. Default `splitOn` is `"Id"` — works when each table has an `Id` column

**Use case**: Loading a task with its assigned user in a single query instead of N+1 separate queries.

### Multiple Result Sets

Execute a single SQL batch that returns multiple result sets. Read each result set sequentially using `QueryMultipleAsync()`:

```
Result Set 1: Tasks for user → Read<TaskItem>()
Result Set 2: User profile   → ReadSingle<User>()
Result Set 3: Notifications  → Read<Notification>()
```

**Advantage**: One database round-trip instead of three separate queries.

### Stored Procedures

Dapper supports stored procedures through `commandType: CommandType.StoredProcedure`. Use `DynamicParameters` for:
- Input parameters
- Output parameters (read after execution with `.Get<T>()`)
- Return values

### Bulk Operations

Passing a collection to `ExecuteAsync()` executes the command once per item. For true bulk operations at scale, use `SqlBulkCopy` (SQL Server) or equivalent provider-specific bulk insert mechanisms.

| Approach | Speed | Use When |
|---|---|---|
| Loop with `ExecuteAsync` | Slow | < 100 rows |
| Collection with `ExecuteAsync` | Moderate | 100–10,000 rows |
| `SqlBulkCopy` | Fast | > 10,000 rows |

### Transactions

Dapper supports transactions through `IDbTransaction`:
1. Open connection
2. Begin transaction
3. Pass transaction to each Dapper call
4. Commit (or rollback on error)

All operations within a transaction are atomic — either all succeed or all are rolled back.

## Design Patterns with Dapper

### Repository Pattern

Encapsulate all data access for an entity type in a repository class:
- Accepts `IDbConnection` (or a connection factory) via constructor injection
- Each method contains the SQL and mapping logic
- Controller/service depends on `IRepository` interface, not Dapper directly

**Benefits**: Testability (mock the interface), centralized SQL, separation of concerns.

### Connection Factory Pattern

Instead of injecting `IDbConnection` directly, inject an `IDbConnectionFactory` that creates new connections on demand. This ensures each operation/scope gets a properly scoped connection.

### Unit of Work Pattern

Coordinates multiple repository operations within a single transaction:
- Manages the shared `IDbConnection` and `IDbTransaction`
- Commits or rolls back all changes together
- Repositories use the shared connection/transaction

### Generic Repository

Base class with common CRUD operations:
- `GetAllAsync()`, `GetByIdAsync()`, `CreateAsync()`, `UpdateAsync()`, `DeleteAsync()`
- Subclasses specify table name and add entity-specific queries
- **Caution**: Can become a leaky abstraction — complex queries often don't fit the generic pattern

### Specification Pattern

Encapsulate query criteria into objects:
- Each specification produces a SQL WHERE clause and parameters
- Specifications can be combined (AND, OR)
- Keeps complex query logic testable and reusable

## Error Handling

### Common Exceptions

| Exception | Cause | Prevention |
|---|---|---|
| `SqlException` | Database errors (constraint violations, timeouts) | Validate input, set proper timeouts |
| `InvalidOperationException` | Wrong query method (e.g., `QuerySingle` with 0 results) | Use appropriate method variant |
| `DataException` | Type mapping failure | Ensure SQL types match .NET types |

### Retry Strategies

Transient database errors (network blips, deadlocks) should be retried:
- **Exponential backoff** — Wait 1s, 2s, 4s, 8s between retries
- **Identify transient errors** — Not all SQL errors are retryable (constraint violations aren't)
- **Libraries** — Polly is the standard .NET resilience library for retry policies
- **Max retries** — Typically 3 attempts before failing permanently

## Performance Optimization

### Connection Pooling
- Enabled by default in ADO.NET providers
- Pool size configurable in connection string (`Max Pool Size=100`)
- Always dispose connections promptly to return them to the pool

### Query Optimization
- Use parameterized queries (enables SQL plan caching)
- Select only needed columns, not `SELECT *`
- Use appropriate indexes
- Monitor query execution plans
- Avoid N+1 queries — use JOINs or multi-mapping

### Caching Strategies

| Strategy | When to Use |
|---|---|
| **In-memory cache** | Frequently read, rarely changed data (lookup tables, config) |
| **Distributed cache** | Multi-server deployments (Redis, SQL Server cache) |
| **Query result cache** | Expensive queries with acceptable staleness |
| **No cache** | Real-time data, frequently changing data |

## Testing Strategies

### Unit Testing
- Mock `IDbConnection` or repository interfaces
- Use an in-memory mock repository for service-level tests
- Test repository methods indirectly through the service layer

### Integration Testing
- Use a real database (localdb, Docker container, or test instance)
- Set up and tear down test data for each test
- Verify actual SQL execution and mapping
- Use database transactions that roll back after each test for isolation

### What to Test

| Test Focus | Approach |
|---|---|
| SQL correctness | Integration test with real database |
| Object mapping | Integration test — verify properties are populated |
| Parameter binding | Integration test — verify correct data is written |
| Error handling | Unit test — mock exceptions from connection |
| Business logic | Unit test — mock repository interface |

## Dapper vs EF Core — Migration Considerations

### When Migrating EF → Dapper

| EF Core Concept | Dapper Equivalent |
|---|---|
| `DbContext` | `IDbConnection` + repositories |
| `DbSet<T>` | Repository methods with SQL |
| `.Include()` / lazy loading | JOIN queries + multi-mapping |
| `.SaveChanges()` | Explicit `ExecuteAsync()` calls |
| Change tracking | Manual state management |
| Migrations | SQL migration scripts (FluentMigrator, DbUp) |
| LINQ queries | Raw SQL strings |

### Challenges
- **More code** — You write all SQL and mapping logic
- **No change tracking** — Must explicitly save every change
- **Relationship management** — Manual join mapping for related entities
- **Schema changes** — Manual migration scripts instead of auto-generated

### Benefits  
- **Transparency** — You see every SQL statement
- **Performance** — No ORM overhead, queries are exactly what you write
- **Debugging** — SQL is visible, not generated behind an abstraction
- **Database features** — Direct access to CTEs, window functions, hints, etc.

## SQL Building

For dynamic queries where WHERE clauses change based on input, options include:
- **String concatenation** — Simple but error-prone (never for user input!)
- **SqlBuilder (Dapper extension)** — Template-based dynamic SQL with `/**where**/` placeholders
- **Custom query builders** — Application-specific helpers for common patterns

## Useful NuGet Packages

| Package | Purpose |
|---|---|
| **Dapper** | Core micro-ORM (the only required package) |
| **Dapper.Contrib** | Simple CRUD extensions (Get, Insert, Update, Delete) |
| **Dapper.SqlBuilder** | Dynamic SQL construction with templates |
| **Dapper.Transaction** | Transaction helper methods |
| **FluentMigrator** | Code-based database migrations (replaces EF migrations) |
| **DbUp** | SQL script-based database migrations |
| **Polly** | Retry policies for transient error handling |
| **MiniProfiler.Dapper** | SQL query profiling and performance analysis |

## Best Practices

### Do
- **Always use parameterized queries** — prevents SQL injection
- **Dispose connections properly** — use `using` or DI scoped lifetime
- **Use async methods** — `QueryAsync`, `ExecuteAsync` for I/O-bound operations
- **Keep SQL in repository classes** — not scattered across services/controllers
- **Use transactions** for multi-statement operations
- **Monitor query performance** — log slow queries, review execution plans
- **Use connection pooling** — enabled by default, don't disable it

### Don't
- **Concatenate user input into SQL** — always parameterize
- **Hold connections open unnecessarily** — acquire late, release early
- **Use `SELECT *`** — select only needed columns
- **Ignore exception handling** — wrap database calls with appropriate error handling
- **Over-abstract** — Dapper's value is simplicity; don't build a mini-ORM on top of it
- **Forget about indexes** — Dapper makes fast queries, but bad queries are still bad

---

*Dapper provides the performance of raw ADO.NET with the convenience of automatic object mapping. Its philosophy is simple: you write SQL, Dapper handles the mapping. This transparency makes it ideal for performance-critical applications where you need full control over data access.*