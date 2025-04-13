- [Key Concepts](#key-concepts)
- [Getting Started](#getting-started)
	- [Prerequisites](#prerequisites)
- [Authentication](#authentication)
- [Initialization](#initialization)
	- [Initialization Scenarios](#initialization-scenarios)
- [Supported Telemetry Types](#supported-telemetry-types)
- [Collecting Telemetry](#collecting-telemetry)
- [Telemetry Publishing](#telemetry-publishing)
- [Using Telemetry Tags](#using-telemetry-tags)
- [Tracking Telemetry](#tracking-telemetry)
	- [Dependency Telemetry](#dependency-telemetry)
- [Distributed Tracing](#distributed-tracing)
	- [How It Works](#how-it-works)
	- [Using Activity Scope](#using-activity-scope)
- [Thread Safety](#thread-safety)
- [Examples](#examples)
	- [Initialize](#initialize)
	- [Initialize with Authentication](#initialize-with-authentication)
	- [Initialize with Multiple Destinations](#initialize-with-multiple-destinations)
	- [Dependency Tracking](#dependency-tracking)
	- [Dependency Tracking via HTTP Client Handler](#dependency-tracking-via-http-client-handler)

## Key Concepts

The following core concepts define the architecture and behavior of this library:

- **Telemetry Tracking** – The process of capturing telemetry data such as events, exceptions, metrics, traces, and activities.
- **Telemetry Publishing** – The process of sending collected telemetry data to one or more instances of a telemetry management service.
- **Distributed Tracing** – A technique for associating telemetry with a logical operation that spans multiple services, processes, or asynchronous flows, enabling end-to-end correlation within a distributed IT solution.

## Getting Started

The library works with Azure [Application Insights][app_insights_info], a feature of Azure [Monitor][azure_montior_info].

### Prerequisites

To use the library, an [Azure subscription][azure_subscription] and an instance of Application Insights resource are required.

The Application Insights resource can be created using 
[Bicep][app_insights_create_bicep],
[CLI][app_insights_create_cli],
[Portal][app_insights_create_portal],
[Powershell][app_insights_create_ps].

## Authentication

Application Insights supports secure access via [Entra authentication][app_insights_entra_auth].

- The Entra identity that executes the code must be assigned the [Monitoring Metrics Publisher][azure_rbac_monitoring_metrics_publisher] role.
- The access token must be requested with the audience: https://monitor.azure.com//.default

## Initialization

The `TelemetryClient` class is the core component for tracking and publishing telemetry.

To enable publishing of telemetry to Application Insights, provide the `TelemetryClient` constructor with one or more instance of types that implement the `TelemetryPublisher` interface.

The library includes `HttpTelemetryPublisher`, an implementation of `TelemetryPublisher` that uses HTTPS protocol for communication.

### Initialization Scenarios

- **Basic (no authentication):**  
  Initialize `TelemetryClient` with a single `HttpTelemetryPublisher`.<br/>
  Refer to the [example](#initialize) below.
- **Entra-based authentication:**  
  Configure `HttpTelemetryPublisher` to authenticate using an access token.<br/>
  Refer to the [example](#initialize-with-authentication) below.
- **Multiple Destinations:**  
  Provide multiple instances of `HttpTelemetryPublisher` to send data to different Application Insights resources.<br/>
  Refer to the [example](#initialize-with-multiple-destinations) below.

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

## Collecting Telemetry

The `TelemetryClient` class provides a method `TelemetryClient.Add` that adds an instance of a class that implements the `Telemetry` interface into the processing queue.

```csharp
// create telemetry item
var telemetry = new TraceTelemetry
{
	Message = "TEST",
	SeverityLevel = SeverityLevel.Verbose,
	Time = DateTime.UtcNow
};

// add to the telemetry client
telemetryClient.Add(telemetry);
```

## Telemetry Publishing

The library delegates telemetry publishing to the developer.</br>
No automated telemetry publishing is provided out of the box.

To publish collected telemetry, use the `TelemetryClient.PublishAsync` method to publish telemetry using all configured publishers in parallel.

```csharp
// publish collected telemetry
await telemetryClient.PublishAsync(cancellationToken);
```

## Using Telemetry Tags

Telemetry tags enrich telemetry data.

Tags are key-value pairs of string type.

The list of well-known standard keys of tags can be found in [TelemetryTagKeys](/src/Code/TelemetryTagKeys.cs) class.

The library provides special helper class [TelemetryTags](/src/Code/TelemetryTags.cs) to simplify work with collections and dictionaries of telemetry tags.

Tags can be applied in the following ways:
- Via `Telemetry.Tags` property – Set during creation.
- Via `TelemetryClient` constructor – Pass instance of `TelemetryTags` to initialize `TelemetryClient.Context`.
- Via `TelemetryClient.Context` Property – Set whenever needed.

## Tracking Telemetry

The `TelemetryClient` class provides a set of `TelemetryClient.Track*` methods.

When telemetry is tracked with `TelemetryClient.Track*` method, the `TelemetryClient.Tags` property is set to the value of `TelemetryClient.Context` property. 

```csharp
// track event and associate with current distributed operation
telemetryClient.TrackEvent("start");
```

### Dependency Telemetry

The library delegates dependency tracking to the developer.<br/>
No automated dependency tracking is provided out of the box.

To track dependencies, either use corresponding `TelemetryClient.TrackDependency*` method or create instance of [DependencyTelemetry](/src/Code/Models/DependencyTelemetry.cs) class and use `TelemetryClient.Add` method.<br/>
Refer to the [example](#dependency-tracking) below.

For a well-known dependency types refer to the [DependencyTypes](/src/Code/Models/DependencyTypes.cs).

The library provides [TelemetryTrackedHttpClientHandler](/src/Code/Dependency/TelemetryTrackedHttpClientHandler.cs) class that helps track HTTP requests.<br/>
Refer to the [example](#dependency-tracking-via-http-client-handler).

## Distributed Tracing

The library is intentionally designed to support scenarios where a single logical flow crosses service, process, or async boundaries.

### How It Works
- The Application Insights supports following telemetry tags
  - OperationId - the unique identifier of the operation that spans across services or components.
  - OperationParentId - the unique identifier of the operation which is parent to the specific telemetry item.
- The telemetry types which represents an activity have an `ActivityTelemetry.Id` property - the unique identifier.<br/>The Id property is used as OperationParentId tag for all subsequent telemetries within the activity scope.
- The `TelemetryClient` class provides a `TelemetryClient.Context` property that holds a reference to the set of telemetry tags including OperationId and OperationName.
- The `TelemetryClient.Context` utilizes [AsyncLocal\<T\>][dot_net_async_local_info], this makes the referenced `TelemetryOperation` available across asynchronous contexts.

### Using Activity Scope

The `TelemetryClient` provides a set of `TelemetryClient.ActivityScope*` methods that simplify work with OperationParentId property of `TelemetryClient.Context`.

The `TelemetryClient.ActivityScopeBegin` begins the scope and `TelemetryClient.ActivityScopeEnd` ends the scope accordingly.

Any telemetry captured within the scope will have scope's activity id as parent operation id.

Nested activity scopes are supported.

The sample below demonstrates use of methods for tracking dependency of in-proc type.<br/>
Refer to the [example](#dependency-tracking) below.

## Thread Safety

All public types and members of this library are **thread-safe** and are safe for concurrent use across multiple threads.

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

// create an HTTP client for the telemetry publisher
using var httpClient = new HttpClient();

// create a telemetry publisher with the specified ingestion endpoint and instrumentation key
var telemetryPublisher = new HttpTelemetryPublisher
(
	httpClient,
	new Uri("INSERT INGESTION ENDPOINT HERE"),
	new Guid("INSERT INSTRUMENTATION KEY HERE")
);

// initialize the tags for the telemetry client context
var tags = new TelemetryTags()
{
	CloudRole = "local",
	CloudRoleInstance = Environment.MachineName,
};

// create a telemetry client using the telemetry publisher and tags
var telemetryClient = new TelemetryClient(telemetryPublisher, tags);

// track a sample event
telemetryClient.TrackEvent("SampleEvent");

// publish the collected telemetry data
_ = await telemetryClient.PublishAsync();
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

// create an HTTP client for the telemetry publisher
using var httpClient = new HttpClient();

// create a token credential source for authorization
var tokenCredential = new DefaultAzureCredential();

// create a telemetry publisher with Entra authentication
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

// create a telemetry client using the telemetry publisher
var telemetryClient = new TelemetryClient(telemetryPublisher);

// track a sample event
telemetryClient.TrackEvent("SampleEvent");

// publish the collected telemetry data
_ = await telemetryClient.PublishAsync();
```

### Initialize with Multiple Destinations

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

// track a sample event
telemetryClient.TrackEvent("SampleEvent");

// publish the collected telemetry data
_ = await telemetryClient.PublishAsync();
```

### Dependency Tracking

The following example demonstrates tracking of any operations as in-proc dependency.

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

// create an HTTP client for the telemetry publisher
using var httpClient = new HttpClient();

// create a token credential source for authorization
var tokenCredential = new DefaultAzureCredential();

// create a telemetry publisher with Entra authentication
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

// create a telemetry client using the telemetry publisher
var telemetryClient = new TelemetryClient(telemetryPublisher);

// sample function that generates a new activity ID
var getActivityId = () => Guid.NewGuid().ToString();

// begin a dependency activity scope
telemetryClient.ActivityScopeBegin(getActivityId, out var time, out var timestamp, out var activityId, out var actualOperation);

// operations that should be tracked
// ....................................

// a variable that indicates if the operation was successful
var success = true;

// end the activity scope
telemetryClient.ActivityScopeEnd(actualOperation, timestamp, out var duration);

// track the dependency as an in-process operation
telemetryClient.TrackDependencyInProc(time, duration, activityId, "Envelope", success, "Custom");

// publish the collected telemetry data
_ = await telemetryClient.PublishAsync();

```

### Dependency Tracking via HTTP Client Handler

The following example demonstrates tracking of HTTP requests of `HttpClient` class with `TelemetryTrackedHttpClientHandler` class.

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

// create an HTTP client for the telemetry publisher
using var telemetryHttpClient = new HttpClient();

// create a token credential source for authorization
var tokenCredential = new DefaultAzureCredential();

// create a telemetry publisher with Entra authentication
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

// create a telemetry client using the telemetry publisher
var telemetryClient = new TelemetryClient(telemetryPublisher);

// sample function that generates a new activity ID
var getActivityId = () => Guid.NewGuid().ToString();

// create an HTTP client handler that tracks dependencies
var handler = new TelemetryTrackedHttpClientHandler(telemetryClient, getActivityId);

// create an HTTP client using the telemetry-tracked handler
using var httpClient = new HttpClient(handler);

// execute an HTTP GET request and track request telemetry
_ = await httpClient.GetAsync("INSERT HTTP URI HERE");

// publish the collected telemetry data
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
