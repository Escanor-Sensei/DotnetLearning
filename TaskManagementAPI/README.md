# Task Management API

A comprehensive .NET 8 Web API for task management with CRUD operations, built following best practices and clean architecture principles.

## 🚀 Features

- **Complete CRUD Operations**: Create, Read, Update, and Delete tasks
- **In-Memory Database**: Using Entity Framework Core with InMemory provider
- **Clean Architecture**: Separated concerns with Controllers, Services, and Data layers
- **Data Transfer Objects (DTOs)**: Proper input/output models for API contracts
- **Validation**: Built-in model validation with DataAnnotations
- **Swagger Documentation**: Interactive API documentation
- **Logging**: Structured logging with different log levels
- **Error Handling**: Proper HTTP status codes and error responses
- **Filtering**: Get tasks by status and priority
- **Seeded Data**: Pre-populated with sample tasks for testing

## 🏗️ Project Structure

```
TaskManagementAPI/
├── Controllers/
│   └── TasksController.cs          # API endpoints and HTTP handling
├── Models/
│   ├── TaskItem.cs                 # Main entity model
│   └── DTOs/
│       └── TaskItemDto.cs          # Data transfer objects
├── Services/
│   ├── ITaskService.cs             # Service interface
│   └── TaskService.cs              # Business logic implementation
├── Data/
│   ├── TaskDbContext.cs            # Entity Framework context
│   └── DataSeed.cs                 # Initial data seeding
├── Properties/
│   └── launchSettings.json         # Development launch configuration
├── appsettings.json                # Application configuration
├── appsettings.Development.json    # Development-specific settings
├── Program.cs                      # Application entry point and service configuration
└── TaskManagementAPI.csproj       # Project file
```

## 📊 Data Model

### TaskItem Entity
- **Id**: Unique identifier (int)
- **Title**: Task title (required, max 100 characters)
- **Description**: Task description (optional, max 500 characters)
- **IsCompleted**: Completion status (bool)
- **Priority**: Task priority (enum: Low, Medium, High, Critical)
- **CreatedAt**: Creation timestamp (DateTime)
- **UpdatedAt**: Last update timestamp (DateTime?)
- **DueDate**: Optional due date (DateTime?)

### TaskPriority Enum
- Low = 1
- Medium = 2
- High = 3
- Critical = 4

## 🔧 API Endpoints

### Base URL: `/api/tasks`

| Method | Endpoint | Description |
|--------|----------|-----------|
| GET | `/` | Get all tasks |
| GET | `/{id}` | Get task by ID |
| POST | `/` | Create new task |
| PUT | `/{id}` | Update existing task |
| DELETE | `/{id}` | Delete task |
| GET | `/filter/status?completed={bool}` | Get tasks by completion status |
| GET | `/filter/priority/{priority}` | Get tasks by priority level |

### Request/Response Examples

#### Create Task (POST /)
```json
{
  "title": "Complete API documentation",
  "description": "Write comprehensive documentation for all endpoints",
  "priority": 2,
  "dueDate": "2024-12-31T23:59:59Z"
}
```

#### Update Task (PUT /{id})
```json
{
  "title": "Complete API documentation",
  "description": "Write comprehensive documentation for all endpoints",
  "isCompleted": true,
  "priority": 2,
  "dueDate": "2024-12-31T23:59:59Z"
}
```

#### Task Response
```json
{
  "id": 1,
  "title": "Complete API documentation",
  "description": "Write comprehensive documentation for all endpoints",
  "isCompleted": false,
  "priority": 2,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null,
  "dueDate": "2024-12-31T23:59:59Z"
}
```

## 🛠️ Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code

### Running the Application

1. **Clone and navigate to the project**:
   ```bash
   cd "TaskManagementAPI"
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```

4. **Access the API**:
   - Swagger UI: `https://localhost:7137/swagger`
   - HTTP: `http://localhost:5137`
   - HTTPS: `https://localhost:7137`

### Testing with Sample Data

The application automatically seeds the database with sample tasks on startup:
- ✅ Complete project setup (High priority, Completed)
- 🔄 Implement CRUD operations (Critical priority, In Progress)
- 📝 Add unit tests (Medium priority, Pending)
- 📖 Document API endpoints (Low priority, Pending)

## 🏛️ Architecture Principles

### 1. **Separation of Concerns**
- **Controllers**: Handle HTTP requests/responses
- **Services**: Contain business logic
- **Data**: Manage data access and persistence

### 2. **Dependency Injection**
- Services are registered in the DI container
- Dependencies are injected through constructors
- Promotes testability and loose coupling

### 3. **Data Transfer Objects (DTOs)**
- Separate models for API contracts (`CreateTaskItemDto`, `UpdateTaskItemDto`)
- Prevents over-posting and under-posting vulnerabilities
- Clean separation between internal and external models

### 4. **Repository Pattern (through EF Core)**
- `TaskDbContext` acts as a repository
- Abstracted data access through `ITaskService`
- Easy to mock for unit testing

### 5. **Configuration Management**
- Environment-specific settings
- Structured logging configuration
- Connection string management

## 🎯 Learning Objectives

This project demonstrates key .NET concepts:

### **ASP.NET Core Web API**
- Controller-based routing
- Action methods and HTTP verbs
- Model binding and validation
- Response status codes

### **Entity Framework Core**
- In-Memory database provider
- DbContext configuration
- Entity relationships and mapping
- Data seeding

### **Dependency Injection**
- Service registration and lifetimes
- Constructor injection
- Interface-based abstractions

### **Best Practices**
- Clean code organization
- Proper error handling
- Logging and monitoring
- API documentation with Swagger

### **Modern C# Features**
- Nullable reference types
- Pattern matching
- LINQ and async/await
- Top-level programs (minimal APIs setup)

## 🚀 Next Steps

Consider extending this project with:

1. **Authentication & Authorization** (JWT tokens)
2. **Real Database** (SQL Server/PostgreSQL)
3. **Unit Tests** (xUnit, MSTest)
4. **Integration Tests** (TestContainers)
5. **Caching** (Redis, In-Memory)
6. **Background Services** (Task reminders)
7. **Rate Limiting** (API throttling)
8. **Health Checks** (System status monitoring)
9. **Docker Support** (Containerization)
10. **API Versioning** (Multiple API versions)

## 📚 Technologies Used

- **Framework**: .NET 8
- **Web Framework**: ASP.NET Core Web API
- **ORM**: Entity Framework Core
- **Database**: In-Memory Database
- **Documentation**: Swagger/OpenAPI
- **Validation**: Data Annotations
- **Logging**: Microsoft.Extensions.Logging

---

*This project serves as a foundation for learning .NET Web API development and can be extended with additional features as needed.*