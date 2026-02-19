using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace TaskManagementAPI.Services;

public interface ITelemetryService
{
    void TrackTaskCreated(int taskId, string priority, bool hasDueDate, string? userId = null);
    void TrackTaskCompleted(int taskId, TimeSpan timeToComplete, string? userId = null);
    void TrackTaskDeleted(int taskId, string? userId = null);
    void TrackUserLogin(string username, string role, bool success);
    void TrackSlowOperation(string operationName, double durationMs, Dictionary<string, string>? properties = null);
    void TrackBusinessMetric(string metricName, double value, Dictionary<string, string>? properties = null);
    IOperationHolder<RequestTelemetry> StartOperation(string operationName);
}

public class TelemetryService : ITelemetryService
{
    private readonly TelemetryClient _telemetryClient;

    public TelemetryService(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void TrackTaskCreated(int taskId, string priority, bool hasDueDate, string? userId = null)
    {
        var properties = new Dictionary<string, string>
        {
            { "TaskId", taskId.ToString() },
            { "Priority", priority },
            { "HasDueDate", hasDueDate.ToString() }
        };

        if (!string.IsNullOrEmpty(userId))
        {
            properties["UserId"] = userId;
        }

        _telemetryClient.TrackEvent("TaskCreated", properties);
        _telemetryClient.TrackMetric("TasksCreated", 1);
    }

    public void TrackTaskCompleted(int taskId, TimeSpan timeToComplete, string? userId = null)
    {
        var properties = new Dictionary<string, string>
        {
            { "TaskId", taskId.ToString() },
            { "CompletionTimeHours", timeToComplete.TotalHours.ToString("F2") }
        };

        if (!string.IsNullOrEmpty(userId))
        {
            properties["UserId"] = userId;
        }

        _telemetryClient.TrackEvent("TaskCompleted", properties);
        _telemetryClient.TrackMetric("TasksCompleted", 1);
        _telemetryClient.TrackMetric("CompletionTimeDays", timeToComplete.TotalDays);
    }

    public void TrackTaskDeleted(int taskId, string? userId = null)
    {
        var properties = new Dictionary<string, string>
        {
            { "TaskId", taskId.ToString() }
        };

        if (!string.IsNullOrEmpty(userId))
        {
            properties["UserId"] = userId;
        }

        _telemetryClient.TrackEvent("TaskDeleted", properties);
        _telemetryClient.TrackMetric("TasksDeleted", 1);
    }

    public void TrackUserLogin(string username, string role, bool success)
    {
        var properties = new Dictionary<string, string>
        {
            { "Username", username },
            { "Role", role },
            { "Success", success.ToString() }
        };

        var eventName = success ? "UserLoginSuccess" : "UserLoginFailed";
        _telemetryClient.TrackEvent(eventName, properties);

        if (success)
        {
            _telemetryClient.TrackMetric($"LoginSuccess_{role}", 1);
        }
        else
        {
            _telemetryClient.TrackMetric("LoginFailures", 1);
        }
    }

    public void TrackSlowOperation(string operationName, double durationMs, Dictionary<string, string>? properties = null)
    {
        var props = properties ?? new Dictionary<string, string>();
        props["OperationName"] = operationName;
        props["DurationMs"] = durationMs.ToString("F2");

        _telemetryClient.TrackEvent("SlowOperation", props);
        _telemetryClient.TrackMetric($"Duration_{operationName}", durationMs);
    }

    public void TrackBusinessMetric(string metricName, double value, Dictionary<string, string>? properties = null)
    {
        _telemetryClient.TrackMetric(metricName, value, properties);
    }

    public IOperationHolder<RequestTelemetry> StartOperation(string operationName)
    {
        return _telemetryClient.StartOperation<RequestTelemetry>(operationName);
    }
}