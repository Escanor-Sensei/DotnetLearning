# Application Insights — Monitoring & Telemetry

## What is Application Performance Monitoring (APM)?

APM is the practice of tracking and analyzing application behavior in real-time to detect performance bottlenecks, failures, and usage patterns. It answers questions like: "Why is the app slow?", "What caused that 3 AM crash?", "Which features do users actually use?"

Application Insights is Microsoft Azure's APM service — it collects, stores, and visualizes telemetry data from your applications.

## Core Concepts

### The Observability Triad

Modern monitoring relies on three pillars:

| Pillar | What It Is | Example |
|---|---|---|
| **Logs** | Discrete events with context | "User 123 created task at 14:30" |
| **Metrics** | Numerical measurements over time | "Average response time: 200ms" |
| **Traces** | End-to-end request journey | Request → Service A → Database → Service B → Response |

Application Insights unifies all three pillars into a single platform.

### Telemetry Data Flow

```
Your Application → SDK (collects data) → Ingestion Endpoint → Azure Storage → Analytics & Dashboards → Alerts
```

The SDK runs inside your application process. It hooks into the ASP.NET Core pipeline to automatically capture request data, then sends it asynchronously to Azure so it doesn't impact application performance.

## Types of Telemetry

### Automatic Telemetry (Zero-Code)

Once the SDK is added, these are captured without writing any additional code:

| Type | What It Captures | Why It Matters |
|---|---|---|
| **Requests** | Every HTTP request — URL, status code, duration, response size | Identifies slow or failing endpoints |
| **Dependencies** | Outgoing calls — SQL queries, HTTP calls, Redis, Azure services | Reveals external bottleneck sources |
| **Exceptions** | Unhandled exceptions with full stack traces | Root cause analysis |
| **Performance Counters** | CPU, memory, GC, thread pool stats | Infrastructure-level health |

### Custom Telemetry (Developer-Defined)

You instrument your code to track business-specific data:

| Type | Purpose | Use Case |
|---|---|---|
| **Custom Events** | Track discrete business actions | "OrderPlaced", "UserRegistered", "TaskCompleted" |
| **Custom Metrics** | Track numerical business values | Revenue per minute, active users, queue depth |
| **Custom Traces** | Detailed diagnostic logging | Debug information, state transitions |
| **Custom Dependencies** | Track calls to systems not auto-detected | Internal microservice calls, custom protocols |

### Event vs Metric — When to Use Which?

- **Event** — Something _happened_ (discrete occurrence). Use `TrackEvent()`. Example: "User logged in"
- **Metric** — A _measurement_ (continuous value). Use `TrackMetric()`. Example: "Response time: 150ms"
- Events can carry **custom properties** (key-value string pairs) and **custom measurements** (key-value numeric pairs) for richer analysis

## Key Metrics to Monitor

### The Four Golden Signals (Google SRE)

These are the most critical metrics for any service:

| Signal | What to Watch | Red Flag |
|---|---|---|
| **Latency** | Request duration (p50, p95, p99) | p95 > 2 seconds |
| **Traffic** | Requests per second | Sudden drops or spikes |
| **Errors** | Error rate (% of failed requests) | > 1% error rate |
| **Saturation** | CPU, memory, connection pool usage | > 80% sustained utilization |

### Percentile Thinking

Averages hide problems. A service with 100ms average might have 5% of users experiencing 5-second responses. Always monitor:
- **p50** (median) — Typical user experience
- **p95** — Experience for most users
- **p99** — Worst-case experience (tail latency)

## Distributed Tracing

### What is It?

In microservice architectures, a single user request may traverse multiple services. Distributed tracing connects all these hops into a single visual timeline.

### How It Works

Application Insights automatically generates and propagates:
- **Operation ID** — Unique identifier for the entire end-to-end operation
- **Parent ID** — Links child operations to their parent
- **Correlation headers** — HTTP headers (`traceparent`, `tracestate`) passed between services

This creates a **dependency tree** showing exactly how a request flowed through your system, where time was spent, and where failures occurred.

## Sampling Strategies

Sending 100% of telemetry is expensive at scale. Sampling reduces data volume while maintaining statistical accuracy.

| Strategy | How It Works | Best For |
|---|---|---|
| **Fixed-rate** | Send X% of all telemetry | Predictable data volume |
| **Adaptive** | Automatically adjusts rate based on volume | Production (recommended) |
| **Ingestion** | Server-side filtering after data arrives | Cost optimization post-collection |

**Critical rule**: Errors, exceptions, and key business events should never be sampled — always send 100% of these.

## Kusto Query Language (KQL)

Application Insights data is queried using KQL — a read-only query language optimized for large-scale data exploration.

### Key Concepts

| Concept | Description |
|---|---|
| **Tables** | `requests`, `dependencies`, `exceptions`, `traces`, `customEvents`, `customMetrics` |
| **Pipe operator** | `|` chains operations (like LINQ) |
| **Tabular output** | Every query produces a table of results |

### Common Query Patterns

- **Filter** — `requests | where success == false` — Find failed requests
- **Aggregate** — `requests | summarize count() by name` — Count requests by endpoint
- **Time series** — `requests | summarize avg(duration) by bin(timestamp, 1h)` — Hourly average response time
- **Join** — Correlate requests with their exceptions or dependencies
- **Render** — `| render timechart` — Visualize as a chart

### Common Analysis Scenarios

| Scenario | Approach |
|---|---|
| Find slow endpoints | Filter `requests` by `duration > threshold`, group by `name` |
| Exception patterns | Group `exceptions` by `type` and `method`, sort by count |
| Dependency bottlenecks | Filter `dependencies` by `success == false` or high `duration` |
| User journey | Filter by `session_id` or `user_id`, sort by `timestamp` |

## Alerting

### Alert Types

| Type | Trigger | Example |
|---|---|---|
| **Metric alerts** | When a metric crosses a threshold | CPU > 80% for 10 minutes |
| **Log alerts** | When a KQL query returns results | More than 5 exceptions in 5 minutes |
| **Smart Detection** | AI detects anomalies automatically | Sudden spike in failure rate |
| **Availability alerts** | Synthetic tests fail | Website unreachable from 2+ regions |

### Alert Design Principles
- **Actionable** — Every alert should have a clear response action
- **Meaningful** — Avoid alert fatigue (too many low-priority alerts)
- **Timely** — Detect issues before users report them
- **Contextual** — Include enough information to begin investigation immediately

## Application Map

The Application Map is an automatically generated visual topology showing:
- All components of your application (web app, APIs, databases, external services)
- Communication paths between them
- Health status of each component (green/yellow/red)
- Call rates and failure rates on each connection

This gives an instant overview of system health without writing any queries.

## Live Metrics Stream

Live Metrics provides a real-time dashboard with ~1 second latency showing:
- Request rate, failure rate, and response time as they happen
- Server health (CPU, memory)
- Recent failures and exceptions
- Useful for monitoring deployments in real-time

## Development vs Production

| Aspect | Development | Production |
|---|---|---|
| **Sampling** | Low rate (10%) or disabled | Adaptive sampling enabled |
| **Detail level** | Full telemetry for debugging | Sampled for cost control |
| **Connection** | Can use local/dev instance | Secure connection string via Key Vault |
| **Alerts** | Disabled or email only | Full alert pipeline (PagerDuty, Teams, etc.) |
| **Retention** | Short (7 days) | Long (90+ days) |

## High-Level Integration Steps

1. **Add SDK** — Install `Microsoft.ApplicationInsights.AspNetCore` NuGet package
2. **Register service** — Call `AddApplicationInsightsTelemetry()` in DI setup
3. **Configure connection** — Set connection string in `appsettings.json` or environment variables
4. **Inject TelemetryClient** — Use DI to access telemetry in services/controllers
5. **Track custom data** — Call `TrackEvent()`, `TrackMetric()`, `TrackException()` for business-specific monitoring
6. **Set up alerts** — Configure metric and log-based alerts in Azure Portal
7. **Query & visualize** — Use KQL in Azure Portal for analysis

## Integration with ILogger

Application Insights automatically captures `ILogger` output. This means your existing structured logging statements become Application Insights traces without code changes. Use `ILogger` for application-level logging — it flows into Application Insights automatically when the SDK is configured.

## Best Practices

### Do
- Track **business events**, not just technical metrics (e.g., "OrderPlaced" not just "POST /api/orders returned 200")
- Use **structured logging** with named parameters (`"Task {TaskId} created by {UserId}"`) for queryable properties
- Set up **correlation IDs** to trace requests across services
- Configure **adaptive sampling** in production to control costs
- Monitor **dependencies** — most performance issues come from external calls

### Don't
- Log **sensitive data** (passwords, PII, credit card numbers) into telemetry
- **Over-instrument** every method — focus on boundaries (incoming requests, outgoing calls, business events)
- Rely only on **averages** — always monitor percentiles
- Ignore the **cost** of telemetry — high-volume apps can generate significant Azure bills
- Set alerts on **non-actionable conditions** — alert fatigue degrades incident response

## Related Concepts

| Concept | Relationship |
|---|---|
| **OpenTelemetry** | Vendor-neutral observability standard; Application Insights supports it as an exporter |
| **Serilog / NLog** | Third-party logging libraries; can sink to Application Insights |
| **Azure Monitor** | Parent platform; Application Insights is one component of Azure Monitor |
| **Prometheus + Grafana** | Open-source alternative for metrics and visualization |

---

*Application Insights transforms your application from a black box into a transparent, observable system. Start with automatic telemetry, then layer in custom events and metrics for business-specific insights.*