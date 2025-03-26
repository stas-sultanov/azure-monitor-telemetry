- [Key Concepts](#key-concepts)
- [Getting Started](#getting-started)
	- [Prerequisites](#prerequisites)
- [Authentication](#authentication)
- [Initialization](#initialization)
	- [Initialization Scenarios](#initialization-scenarios)
- [Supported Telemetry Types](#supported-telemetry-types)
- [Adding Telemetry](#adding-telemetry)
- [Tracking Telemetry](#tracking-telemetry)
	- [Dependency Telemetry](#dependency-telemetry)
- [Publishing](#publishing)
- [Using Telemetry Tags](#using-telemetry-tags)
- [Distributed Operation Tracking](#distributed-operation-tracking)
	- [How It Works](#how-it-works)
	- [Using Activity Scope](#using-activity-scope)
- [Thread Safety](#thread-safety)
- [Examples](#examples)
	- [Initialize](#initialize)
	- [Initialize with Authentication](#initialize-with-authentication)
	- [Initialize with Multiple Publishers](#initialize-with-multiple-publishers)
	- [Dependency Tracking](#dependency-tracking)
	- [Dependency Tracking via HTTP Client Handler](#dependency-tracking-via-http-client-handler)

## Key Concepts

The following core concepts define the architecture and behavior of this library:

- **Telemetry Tracking** – The process of capturing telemetry data such as events, exceptions, metrics, traces, and activities.
- **Telemetry Publishing** – The process of transmitting collected telemetry data to one or more instances of a telemetry management service.
- **Distributed Tracing** – A technique for associating telemetry with a logical operation that spans multiple services, processes, or asynchronous flows, enabling end-to-end correlation within a distributed IT solution.

## Getting Started

The library works with Azure [Application Insights][app_insights_info], a feature of Azure [Monitor][azure_montior_info].

### Prerequisites

To use the library, an [Azure subscription][azure_subscription] and an Application Insights resource are required.

The Application Insights resource can be created via 
[Bicep][app_insights_create_bicep],
[CLI][app_insights_create_cli],
[Portal][app_insights_create_portal],
[Powershell][app_insights_create_ps].

## Authentication

Application Insights supports secure access via [Entra authentication][app_insights_entra_auth].

- The identity executing the code must be assigned the [Monitoring Metrics Publisher][azure_rbac_monitoring_metrics_publisher] role.
- The access token must be requested with the audience: https://monitor.azure.com//.default

## Initialization

The `TelemetryClient` class is the core component for tracking and publishing telemetry.

To enable telemetry publishing to Application Insights, provide the `TelemetryClient` constructor with one or more implementations of the `TelemetryPublisher` interface.

The library includes `HttpTelemetryPublisher`, an implementation of `TelemetryPublisher` that communicates via HTTPS.

### Initialization Scenarios

- **Basic (no authentication):**  
  Initialize `TelemetryClient` with a single `HttpTelemetryPublisher`.<br/>
  Refer to the [example](#initialize) below.
- **Entra-based authentication:**  
  Configure `HttpTelemetryPublisher` to authenticate using an access token.<br/>
  Refer to the [example](#initialize-with-authentication) below.
- **Multiple Publishers:**  
  Provide multiple instances of `HttpTelemetryPublisher` to send data to different Application Insights resources.<br/>
  Refer to the [example](#initialize-with-multiple-publishers) below.

## Supported Telemetry Types

The library provides support for all telemetry types supported by Application Insights.

- Point-in-time Telemetry:
	- [EventTelemetry](/src/Code/Models/EventTelemetry.cs)
	- [ExceptionTelemetry](/src/Code/Models/ExceptionTelemetry.cs)
	- [MetricTelemetry](/src/Code/Models/MetricTelemetry.cs)
	- [TraceTelemetry](/src/Code/Models/TraceTelemetry.cs)
- Activity Telemetry:
	- [AvailabilityTelemetry](/src/Code/Models/AvailabilityTelemetry.cs)
	- [DependencyTelemetry](/src/Code/Models/DependencyTelemetry.cs)
	- [PageViewTelemetry](/src/Code/Models/PageViewTelemetry.cs)
	- [RequestTelemetry](/src/Code/Models/RequestTelemetry.cs)

## Adding Telemetry

The `TelemetryClient` class provides a method `TelemetryClient.Add` that adds an instance of a class that implements the `Telemetry` interface into the processing queue.

```csharp
// create telemetry item
var telemetry = new EventTelemetry(DateTime.UtcNow, "start");

// add to the telemetryClient
telemetryClient.Add(telemetry);
```

## Publishing

The library delegates telemetry publishing to the developer.</br>
No automated telemetry publishing is provided out of the box.

To publish collected telemetry, use the `TelemetryClient.PublishAsync` method.

```csharp
// publish collected telemetry
await telemetryClient.PublishAsync(cancellationToken);
```

The collected telemetry data will be published in parallel using all configured instances of the `TelemetryPublisher` interface.

## Distributed Operation Tracking

The library is intentionally designed to support distributed operations, where a single logical flow spans services, components, or asynchronous boundaries.

### How It Works

- The [TelemetryOperation](/src/Code/TelemetryOperation.cs) class represents an operation context.
- The `Telemetry` interface provides a `Telemetry.Operation` property that represents the associated operation context.
- The `TelemetryClient` class provides a `TelemetryClient.Operation` property that holds a reference to the current distributed operation context.
- The `TelemetryClient.Operation` utilizes [AsyncLocal\<T\>][dot_net_async_local_info] to store data, this makes the referenced `TelemetryOperation` available across asynchronous contexts.

### Using Activity Scope

The `TelemetryClient` provides a set of `TelemetryClient.ActivityScope*` methods that simplify work with `TelemetryClient.Operation`.

The `TelemetryClient.ActivityScopeBegin` intended to begin activity scope and `TelemetryClient.ActivityScopeEnd` ends the scope accordingly.

Any telemetry captured within the scope will have activity id as it's parent, unless there will be nested activity scope.

The sample below demonstrates use of methods for tracking dependency of in-proc type.<br/>
Refer to the [example](#dependency-tracking) below.

## Tracking Telemetry

The `TelemetryClient` class provides a set of `TelemetryClient.Track*` methods.

When a telemetry is tracked with `TelemetryClient.Track*` method, the `Telemetry.Operation` properties is set to `TelemetryClient.Operation` property value. 

For most cases, Track methods will call `DateTime.UtcNow` to get the current timestamp for the telemetry item.

```csharp
// track event and associate with current distributed operation
telemetryClient.TrackEvent("start");
```

### Dependency Telemetry

The library delegates dependency tracking to the developer.<br/>
No automated dependency tracking is provided out of the box.

To track decency either use corresponding `TelemetryClient.TrackDependency*` method or create instance of `DependencyTelemetry` class and use `TelemetryClient.Add` method.<br/>
Refer to the [example](#dependency-tracking) below.

For a well-known dependency types refer to the [DependencyTypes](/src/Code/Models/DependencyTypes.cs).

The library provides [TelemetryTrackedHttpClientHandler](/src/Code/Dependency/TelemetryTrackedHttpClientHandler.cs) class that helps tracking of HTTP requests.<br/>
Refer to the [example](#dependency-tracking-via-http-client-handler).

## Using Telemetry Tags

Telemetry tags enrich telemetry data with metadata.

It simple key-value pairs of string type.

The list of standard tags can be found in [TelemetryTagKeys](/src/Code/TelemetryTagKeys.cs) class.

Ways to apply tags:
- Per Item – Set tags on object which type implements `Telemetry` interface.
- Per Client - pass tags to the `TelemetryClient` constructor.<br/>
  These tags are automatically included in all telemetry items published by the client.
- Per Publisher - pass tags to the `HttpTelemetryPublisher` constructor.<br/>
  These tags are automatically included in all telemetry items published by specified publisher only.

## Thread Safety

All public types and members of this library are **thread-safe** and can be used concurrently from multiple threads.

- The `TelemetryClient` class is designed to handle telemetry operations from multiple threads in parallel.
- Internal telemetry storage is implemented using [ConcurrentQueue\<T\>][dot_net_concurrent_queue_info] to ensure safe concurrent access.
- The `TelemetryClient.PublishAsync` method can be safely called while telemetry is being added via `TelemetryClient.Add` or `TelemetryClient.Track*` methods.

## Examples

Use this examples to explore functionality of the library.

### Initialize

The following example demonstrates how to initialize the `TelemetryClient`.

Prerequisite
```bash
dotnet add package Stas.Azure.Monitor.Telemetry
```

Code
```csharp
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;

// Create an HTTP client for the telemetry publisher
using var httpClient = new HttpClient();

// Create a telemetry publisher with the specified ingestion endpoint and instrumentation key
var telemetryPublisher = new HttpTelemetryPublisher
(
	httpClient,
	new Uri("INSERT INGESTION ENDPOINT HERE"),
	new Guid("INSERT INSTRUMENTATION KEY HERE")
);

// Create a collection of tags for telemetry data
KeyValuePair<String, String>[] tags = [new(TelemetryTagKeys.CloudRole, "local")];

// Create a telemetry client using the telemetry publisher and tags
var telemetryClient = new TelemetryClient(telemetryPublisher, tags);
```

### Initialize with Authentication

The following example demonstrates how to initialize the `TelemetryClient` with Entra authentication.

Prerequisite
```bash
dotnet add package Stas.Azure.Monitor.Telemetry
dotnet add package Azure.Identity
```

Code
```csharp
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;

// Create an HTTP client for the telemetry publisher
using var httpClient = new HttpClient();

// Create a token credential source for authorization
var tokenCredential = new DefaultAzureCredential();

// Create a telemetry publisher with Entra authentication
var telemetryPublisher = new HttpTelemetryPublisher
(
	httpClient,
	new Uri("INSERT INGESTION ENDPOINT HERE"),
	new Guid("INSERT INSTRUMENTATION KEY HERE"),
	async (cancellationToken) =>
	{
		var tokenRequestContext = new TokenRequestContext([HttpTelemetryPublisher.AuthorizationScope]);
		var token = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken);
		return new BearerToken
		{
			ExpiresOn = token.ExpiresOn,
			Value = token.Token
		};
	}
);

// Create a telemetry client using the telemetry publisher
var telemetryClient = new TelemetryClient(telemetryPublisher);
```

### Initialize with Multiple Publishers

The following example demonstrates initialization of the `TelemetryClient` for the scenario
where it is required to publish telemetry data into multiple instances of Application Insights.

Prerequisite
```bash
dotnet add package Stas.Azure.Monitor.Telemetry
dotnet add package Azure.Identity
```

Code
```csharp
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;

// Create an HTTP client for the telemetry publishers
using var httpClient = new HttpClient();

// Create a token credential source for authorization
var tokenCredential = new DefaultAzureCredential();

// Create the first telemetry publisher with Entra-based authentication
var firstTelemetryPublisher = new HttpTelemetryPublisher
(
	httpClient,
	new Uri("INSERT INGESTION ENDPOINT 1 HERE"),
	new Guid("INSERT INSTRUMENTATION KEY 1 HERE"),
	async (cancellationToken) =>
	{
		var tokenRequestContext = new TokenRequestContext([HttpTelemetryPublisher.AuthorizationScope]);
		var token = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken);
		return new BearerToken
		{
			ExpiresOn = token.ExpiresOn,
			Value = token.Token
		};
	}
);

// Create the second telemetry publisher without Entra-based authentication
var secondTelemetryPublisher = new HttpTelemetryPublisher
(
	httpClient,
	new Uri("INSERT INGESTION ENDPOINT 2 HERE"),
	new Guid("INSERT INSTRUMENTATION KEY 2 HERE")
);

// Create a telemetry client using both telemetry publishers
var telemetryClient = new TelemetryClient([firstTelemetryPublisher, secondTelemetryPublisher]);
```

### Dependency Tracking

The following example demonstrates tracking of any operations as in-proc activity.

Prerequisite
```bash
dotnet add package Stas.Azure.Monitor.Telemetry
dotnet add package Azure.Identity
```

Code
```csharp
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;

// Create an HTTP client for the telemetry publisher
using var httpClient = new HttpClient();

// Create a token credential source for authorization
var tokenCredential = new DefaultAzureCredential();

// Create a telemetry publisher with Entra authentication
var telemetryPublisher = new HttpTelemetryPublisher
(
	httpClient,
	new Uri("INSERT INGESTION ENDPOINT HERE"),
	new Guid("INSERT INSTRUMENTATION KEY HERE"),
	async (cancellationToken) =>
	{
		var tokenRequestContext = new TokenRequestContext([HttpTelemetryPublisher.AuthorizationScope]);
		var token = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken);
		return new BearerToken
		{
			ExpiresOn = token.ExpiresOn,
			Value = token.Token
		};
	}
);

// Create a telemetry client using the telemetry publisher
var telemetryClient = new TelemetryClient(telemetryPublisher);

// Sample function that generates a new activity ID
var getActivityId = () => Guid.NewGuid().ToString();

// Begin a dependency activity scope
telemetryClient.ActivityScopeBegin(getActivityId, out var time, out var timestamp, out var activityId, out var actualOperation);

// Operations that should be tracked
// ....................................

// A variable that indicates if the operation was successful
var success = true;

// End the activity scope
telemetryClient.ActivityScopeEnd(actualOperation, timestamp, out var duration);

// Track the dependency as an in-process operation
telemetryClient.TrackDependencyInProc(time, duration, activityId, "Envelope", success, "Custom");

// Publish the collected telemetry
_ = await telemetryClient.PublishAsync();

```

### Dependency Tracking via HTTP Client Handler

The following example demonstrates tracking of Http request of `HttpClient` class with `TelemetryTrackedHttpClientHandler` class.

Prerequisite
```bash
dotnet add package Stas.Azure.Monitor.Telemetry
dotnet add package Azure.Identity
```

Code
```csharp
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Dependency;
using Azure.Monitor.Telemetry.Publish;

// Create an HTTP client for the telemetry publisher
using var telemetryHttpClient = new HttpClient();

// Create a token credential source for authorization
var tokenCredential = new DefaultAzureCredential();

// Create a telemetry publisher with Entra authentication
var telemetryPublisher = new HttpTelemetryPublisher
(
	telemetryHttpClient,
	new Uri("INSERT INGESTION ENDPOINT HERE"),
	new Guid("INSERT INSTRUMENTATION KEY HERE"),
	async (cancellationToken) =>
	{
		var tokenRequestContext = new TokenRequestContext([HttpTelemetryPublisher.AuthorizationScope]);
		var token = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken);
		return new BearerToken
		{
			ExpiresOn = token.ExpiresOn,
			Value = token.Token
		};
	}
);

// Create a telemetry client using the telemetry publisher
var telemetryClient = new TelemetryClient(telemetryPublisher);

// Sample function that generates a new activity ID
var getActivityId = () => Guid.NewGuid().ToString();

// Create an HTTP client handler that tracks dependencies
var handler = new TelemetryTrackedHttpClientHandler(telemetryClient, getActivityId);

// Create an HTTP client using the telemetry-tracked handler
using var httpClient = new HttpClient(handler);

// Execute an HTTP GET request and track request telemetry
_ = await httpClient.GetAsync("INSERT HTTP URI HERE");

// Publish the collected telemetry
_ = await telemetryClient.PublishAsync();
```

[azure_monitor]: https://docs.microsoft.com/azure/azure-monitor/overview
[azure_montior_info]: https://learn.microsoft.com/azure/azure-monitor/fundamentals/overview
[azure_subscription]: https://azure.microsoft.com/free/dotnet/
[azure_resource_app_insights]: https://learn.microsoft.com/azure/templates/microsoft.insights/components
[azure_rbac_monitoring_metrics_publisher]: https://learn.microsoft.com/azure/role-based-access-control/built-in-roles/monitor#monitoring-metrics-publisher
[app_insights_info]: https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview
[app_insights_entra_auth]: https://learn.microsoft.com/azure/azure-monitor/app/azure-ad-authentication
[app_insights_create_bicep]: https://learn.microsoft.com/azure/azure-monitor/app/create-workspace-resource?tabs=bicep#create-an-application-insights-resource
[app_insights_create_cli]: https://learn.microsoft.com/azure/azure-monitor/app/create-workspace-resource?tabs=cli#create-an-application-insights-resource
[app_insights_create_portal]: https://learn.microsoft.com/azure/azure-monitor/app/create-workspace-resource?tabs=portal#create-an-application-insights-resource
[app_insights_create_ps]: https://learn.microsoft.com/azure/azure-monitor/app/create-workspace-resource?tabs=powershell#create-an-application-insights-resource
[dot_net_async_local_info]: https://learn.microsoft.com/dotnet/api/system.threading.asynclocal-1
[dot_net_concurrent_queue_info]: https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentqueue-1
