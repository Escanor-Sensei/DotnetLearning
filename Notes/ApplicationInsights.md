# Application Insights - Monitoring & Telemetry Guide

## 🔍 **What is Application Insights?**

Application Insights is Microsoft Azure's **Application Performance Monitoring (APM)** service that provides deep insights into your application's performance, usage, and health.

## 🎯 **Why Use Application Insights?**

### **Problems It Solves:**
- ❓ "Why is my app slow?"
- ❓ "Which users are experiencing errors?"
- ❓ "What features are most used?"
- ❓ "When did performance degrade?"
- ❓ "What caused that exception at 3 AM?"

### **Business Value:**
- 📊 **Performance Optimization** - Identify bottlenecks
- 🐛 **Faster Bug Resolution** - Detailed error tracking
- 👥 **User Experience** - Understand user behavior
- 💰 **Cost Optimization** - Resource usage insights
- ⚡ **Proactive Monitoring** - Alerts before users complain

## 🏗️ **How Application Insights Works**

```
Your App → [SDK] → Application Insights → Azure Portal → Insights & Alerts
```

1. **SDK Integration** - Add NuGet package to your app
2. **Automatic Telemetry** - Collects requests, dependencies, exceptions
3. **Custom Telemetry** - Track custom events and metrics
4. **Azure Storage** - Data stored in Azure for analysis
5. **Rich Dashboards** - Visualize performance and usage

## 📊 **Types of Telemetry Collected**

### **Automatically Collected:**
- 🌐 **HTTP Requests** - Response times, status codes, URLs
- ⚠️ **Exceptions** - Stack traces, error details
- 🔗 **Dependencies** - Database calls, HTTP calls, Redis
- 📈 **Performance Counters** - CPU, memory, disk usage
- 💻 **Server Metrics** - Server health and performance

### **Custom Telemetry:**
- 📝 **Custom Events** - User actions, business events
- 📊 **Custom Metrics** - Business KPIs, counters
- 🔍 **Custom Traces** - Detailed logging information
- ⏱️ **Custom Timing** - Specific operation durations

## 🚀 **Basic Implementation**

### **1. Install NuGet Package**
```xml
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.22.0" />
```

### **2. Configure in Program.cs**
```csharp
// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Or with connection string
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
});
```

### **3. Add Configuration**
```json
// appsettings.json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key-here;IngestionEndpoint=https://centralus-0.in.applicationinsights.azure.com/"
  }
}
```

### **4. Use in Controllers**
```csharp
public class TasksController : ControllerBase
{
    private readonly ILogger<TasksController> _logger;
    private readonly TelemetryClient _telemetryClient;

    public TasksController(ILogger<TasksController> logger, TelemetryClient telemetryClient)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        // Track custom event
        _telemetryClient.TrackEvent("TaskCreated", new Dictionary<string, string>
        {
            {"Priority", dto.Priority.ToString()},
            {"HasDueDate", (dto.DueDate != null).ToString()}
        });

        // Track custom metric
        _telemetryClient.TrackMetric("TasksCreatedCount", 1);

        // Your business logic here
        var task = await _taskService.CreateTaskAsync(dto);

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }
}
```

## 📈 **Key Metrics to Monitor**

### **Performance Metrics:**
- **Response Time** - How fast requests are processed
- **Request Rate** - Requests per second/minute
- **Error Rate** - Percentage of failed requests
- **Dependency Duration** - Database/API call times

### **Business Metrics:**
- **User Activity** - Active users, feature usage
- **Conversion Rates** - Business goal completions  
- **Custom KPIs** - Domain-specific metrics

### **Infrastructure Metrics:**
- **CPU Usage** - Server resource utilization
- **Memory Usage** - RAM consumption patterns
- **Disk I/O** - Storage performance

## 🎛️ **Development vs Production Setup**

### **Development (Local Testing)**
```csharp
// Use sampling for development
builder.Services.Configure<TelemetryConfiguration>(config =>
{
    config.DefaultTelemetrySink.TelemetryProcessorChainBuilder
        .UseSampling(10) // Only send 10% of telemetry
        .Build();
});
```

### **Production (Full Monitoring)**
```csharp
// Full telemetry for production
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
    options.EnableAdaptiveSampling = true;
    options.EnableQuickPulseMetricStream = true;
});
```

## 🔧 **Practical Examples**

### **Track User Actions**
```csharp
// Track when user completes a task
_telemetryClient.TrackEvent("TaskCompleted", new Dictionary<string, string>
{
    {"TaskId", task.Id.ToString()},
    {"UserId", userId},
    {"CompletionTime", DateTime.UtcNow.ToString()}
});
```

### **Monitor Performance**
```csharp
// Track custom operation timing
using var operation = _telemetryClient.StartOperation<RequestTelemetry>("ComplexOperation");
try
{
    // Your complex operation here
    await ProcessLargeDataset();
    operation.Telemetry.Success = true;
}
catch (Exception ex)
{
    operation.Telemetry.Success = false;
    _telemetryClient.TrackException(ex);
    throw;
}
```

### **Track Dependencies**
```csharp
// Track external API calls
using var dependency = _telemetryClient.StartOperation<DependencyTelemetry>("ExternalAPI");
dependency.Telemetry.Type = "HTTP";
dependency.Telemetry.Target = "api.external-service.com";

try
{
    var result = await _httpClient.GetAsync("https://api.external-service.com/data");
    dependency.Telemetry.Success = result.IsSuccessStatusCode;
    dependency.Telemetry.ResultCode = result.StatusCode.ToString();
}
catch (Exception ex)
{
    dependency.Telemetry.Success = false;
    _telemetryClient.TrackException(ex);
    throw;
}
```

## 📊 **Useful Queries (KQL - Kusto Query Language)**

### **Top Failed Requests**
```kql
requests
| where success == false
| summarize count() by name, resultCode
| order by count_ desc
| take 10
```

### **Slow Operations**
```kql
requests
| where duration > 5000  // slower than 5 seconds
| project timestamp, name, duration, url
| order by duration desc
```

### **Exception Analysis**
```kql
exceptions
| summarize count() by type, method
| order by count_ desc
```

### **Custom Events Tracking**
```kql
customEvents
| where name == "TaskCreated"
| extend Priority = tostring(customDimensions.Priority)
| summarize count() by Priority
```

## 🚨 **Alerts & Monitoring**

### **Common Alerts to Set Up:**
- 🔴 **High Error Rate** - More than 5% failed requests
- 🐌 **Slow Response Time** - 95th percentile > 2 seconds  
- 💥 **Exception Spike** - 10+ exceptions in 5 minutes
- 📈 **High CPU Usage** - CPU > 80% for 10 minutes
- 📊 **Low Request Volume** - Business hours with no activity

### **Alert Configuration Example:**
```csharp
// Set up alerts in Azure Portal or via ARM templates
{
  "condition": {
    "allOf": [
      {
        "metricName": "requests/failed",
        "metricNamespace": "Microsoft.Insights/components",
        "operator": "GreaterThan",
        "threshold": 5,
        "timeAggregation": "Average"
      }
    ]
  },
  "actions": [
    {
      "actionGroupId": "/subscriptions/{subscription}/resourceGroups/{rg}/providers/microsoft.insights/actionGroups/{actiongroup}"
    }
  ]
}
```

## 💡 **Best Practices**

### **DO's ✅**
- ✅ **Use correlation IDs** for request tracing
- ✅ **Track business metrics** not just technical metrics
- ✅ **Set up meaningful alerts** based on user impact
- ✅ **Use sampling in development** to reduce noise
- ✅ **Add custom properties** to enrich telemetry
- ✅ **Monitor dependencies** (databases, APIs, queues)

### **DON'Ts ❌**
- ❌ **Don't log sensitive data** (passwords, PII)
- ❌ **Don't over-instrument** every single operation
- ❌ **Don't ignore performance impact** of telemetry
- ❌ **Don't forget to configure sampling** for high-volume apps
- ❌ **Don't rely only on technical metrics** without business context

## 🎓 **Learning Benefits**

### **For Developers:**
1. 🔍 **Debugging Skills** - Trace issues across distributed systems
2. 📊 **Performance Awareness** - Understand application bottlenecks
3. 👥 **User Focus** - See applications from user perspective
4. 🛠️ **Monitoring Mindset** - Build observability into applications

### **For Operations:**
1. 🚨 **Proactive Issue Detection** - Know about problems before users
2. 📈 **Capacity Planning** - Understand usage patterns and scaling needs
3. 💰 **Cost Optimization** - Identify resource waste and inefficiencies
4. 🎯 **SLA Management** - Monitor and maintain service level agreements

## 🔄 **Integration with Existing Logging**

Application Insights works alongside your existing logging:

```csharp
// ILogger messages are automatically sent to Application Insights
_logger.LogInformation("Task {TaskId} created by user {UserId}", task.Id, userId);

// This creates both a log entry and Application Insights telemetry
```

## 📚 **Simple Setup Summary**

1. **Add NuGet package** ➜ Microsoft.ApplicationInsights.AspNetCore
2. **Configure service** ➜ AddApplicationInsightsTelemetry()
3. **Add connection string** ➜ In appsettings.json
4. **Inject TelemetryClient** ➜ In controllers/services
5. **Track custom events** ➜ TrackEvent(), TrackMetric()
6. **Set up alerts** ➜ In Azure Portal
7. **Query data** ➜ Using KQL in Azure Portal

---

*Application Insights transforms your application from a "black box" into a transparent, observable system. Start simple with automatic telemetry, then gradually add custom tracking for your specific business needs.*