# Azure Monitor Telemetry

## Getting Started

The library is designed to work with the Azure [Application Insights][app_insights_info], which is a feature of Azure [Monitor][azure_montior_info].

### Prerequisites

To use the library, an [Azure subscription][azure_subscription] and an [Application Insights][app_insights_info] resource are required.

It is possible to create a new **Application Insights** resource via 
[Bicep][app_insights_create_bicep],
[CLI][app_insights_create_cli],
[Portal][app_insights_create_portal],
[Powershell][app_insights_create_ps],
[REST][app_insights_create_rest].

## Authentication

**Application Insights** supports secure access via [Entra authentication][app_insights_entra_auth].

- The identity executing the code must be assigned the [Monitoring Metrics Publisher][azure_rbac_monitoring_metrics_publisher] role.
- The access token must be requested with the audience: `https://monitor.azure.com//.default`.

## Initialization

The `TelemetryClient` class is the core component for tracking and publishing telemetry.

To send telemetry to Application Insights, provide the `TelemetryClient` constructor with one or more implementations of the `TelemetryPublisher` interface.

The library includes `HttpTelemetryPublisher`, an implementation of `TelemetryPublisher` that communicates via HTTPS.

### Initialization Scenarios

- **Basic (no authentication):**  
  Initialize `TelemetryClient` with a single `HttpTelemetryPublisher`.
  Take a look at the [example](#init-with-single-publisher).

- **Entra-based authentication:**  
  Configure `HttpTelemetryPublisher` to authenticate using an access token.
  Take a look at the [example](#init-with-entra-auth).

- **Multiple endpoints:**  
  Provide multiple instances of `HttpTelemetryPublisher` to send data to different Application Insights resources.
  Take a look at the [example](#init-with-multiple-publishers).

## Supported Telemetry Types

The library provides support for all telemetry types supported by **Application Insights**.

- Timestamp-based telemetry:
	- [EventTelemetry](/src/Code/Models/EventTelemetry.cs)
	- [ExceptionTelemetry](/src/Code/Models/ExceptionTelemetry.cs)
	- [MetricTelemetry](/src/Code/Models/MetricTelemetry.cs)
	- [TraceTelemetry](/src/Code/Models/TraceTelemetry.cs)
- Activity-based telemetry:
	- [AvailabilityTelemetry](/src/Code/Models/AvailabilityTelemetry.cs)
	- [DependencyTelemetry](/src/Code/Models/DependencyTelemetry.cs)
	- [PageViewTelemetry](/src/Code/Models/PageViewTelemetry.cs)
	- [RequestTelemetry](/src/Code/Models/RequestTelemetry.cs)

## Adding Telemetry

The `TelemetryClient` class provides a method `Add` that adds an instance of a class that implements the `Telemetry` interface into the processing queue.

```csharp
// create telemetry item
var telemetry = new EventTelemetry(DateTime.UtcNow, "start");

// add to the telemetryClient
telemetryClient.Add(telemetry);
```

## Tracking Telemetry

The `TelemetryClient` class provides a set of `Track` methods.
The purpose of these methods is to simplify adding telemetry to the `TelemetryClient` storage.
The major point of `Track` methods is to associate telemetry items with the distributed operation that is currently tracked by `TelemetryClient`.
For most cases, Track methods will call `DateTime.UtcNow` to get the current timestamp for the telemetry item.

```csharp
// track event and associate with current distributed operation
telemetryClient.TrackEvent("start");
```

### Dependency Tracking

The library does not provide any automated dependency tracking.

The library provides [TelemetryTrackedHttpClientHandler](/src/Code/Dependency/TelemetryTrackedHttpClientHandler.cs) class that allows tracking of HTTP requests.

The [code sample](#init-with-multiple-publishers) demonstrates use of **TelemetryTrackedHttpClientHandler** class to track calls by Azure Storage Queue Client.

## Publishing

To publish collected telemetry, use the `TelemetryClient.PublishAsync` method.

```csharp
// publish collected telemetry
await telemetryClient.PublishAsync(cancellationToken);
```

The collected telemetry data will be published in parallel using all configured instances of the `TelemetryPublisher` interface.

The library does not provide any functionality of publishing data automatically.

## Using Telemetry Tags

Telemetry tags enrich telemetry data with metadata.

The list of standard tags can be found in [TelemetryTagKeys](/src/Code/TelemetryTagKeys.cs) class.

Ways to apply tags:
- Per instance of object which type implements `Telemetry` interface.
  Set the `Telemetry.Tags` property on the instance.
- Per client
  Pass tags to the `TelemetryClient` constructor.
  These tags are applied to each telemetry at publish time.
- Per publisher
  Pass tags to the `HttpTelemetryPublisher` constructor.
  These tags are applied to each telemetry at publish time for specific publisher only.

## Distributed Operation Tracking

This library is purpose-built to support **distributed operations**, where a single logical transaction spans multiple components, services, or asynchronous workflows.

To support this, the `TelemetryClient` class provides an `Operation` property that represents the current logical operation.
Telemetry items tracked while an operation is active will automatically be associated with it.

### How It Works

- The `Operation` property holds a reference to the current distributed operation context.
- When a telemetry item is added or tracked, it will inherit operation identifiers (`Id`, `Name`, `ParentId`) from this context.
- This enables **end-to-end correlation** in Application Insights across services.

## Thread Safety

All public types and members of this library are **thread-safe** and can be used concurrently from multiple threads.

- The `TelemetryClient` class is designed to handle telemetry operations from multiple threads in parallel.
- Internal telemetry storage is implemented using `ConcurrentQueue<T>` to ensure safe concurrent access.
- The `PublishAsync` method can be safely called while telemetry is being added via `Add` or `Track*` methods.
- Custom implementations of `TelemetryPublisher` should also be designed to be thread-safe, especially if shared across multiple instances or services.

This thread-safe design makes the library suitable for use in **high-concurrency environments**, such as web servers, background workers, microservices, and serverless applications.

## Examples

### Init with Single Publisher

Example demonstrates initialization of `TelemetryClient` with one publisher.

```csharp
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;

// create an HTTP Client for telemetry publisher
using var httpClient = new HttpClient();

// create telemetry publisher
var telemetryPublisher = new HttpTelemetryPublisher
(
	httpClient,
	new Uri("INSERT INGESTION ENDPOINT HERE"),
	new Guid("INSERT INSTRUMENTATION KEY HERE")
);

// create tags collection
KeyValuePair<String, String> [] tags = [new (TelemetryTagKey.CloudRole, "local")];

// create telemetry Telemetry Client
var telemetryClient = new TelemetryClient(tags, telemetryPublishers: telemetryPublisher);
```

### Init with Entra Auth

Example demonstrates initialization of `TelemetryClient` with Entra authentication.

```csharp
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;

// create an HTTP Client for telemetry publisher
using var httpClient = new HttpClient();

// create authorization token source
var tokenCredential = new DefaultAzureCredential();

// Create telemetry publisher with Entra authentication
var telemetryPublisher = new HttpTelemetryPublisher
(
	httpClient,
	new Uri("INSERT INGESTION ENDPOINT HERE"),
	new Guid("INSERT INSTRUMENTATION KEY HERE"),
	async (cancellationToken) =>
	{
		var tokenRequestContext = new TokenRequestContext(HttpTelemetryPublisher.AuthorizationScopes);
		var token = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken);
		return new BearerToken(token.Token, token.ExpiresOn);
	}
);

// create telemetry Telemetry Client
var telemetryClient = new TelemetryClient(telemetryPublishers: telemetryPublisher);
```

### Init with Multiple Publishers

The code sample below demonstrates initialization of the `TelemetryClient` for the scenario
where it is required to publish telemetry data into multiple instances of **Application Insights**.

```csharp
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;

// create an HTTP Client for telemetry publisher
using var httpClient = new HttpClient();

// create authorization token source
var tokenCredential = new DefaultAzureCredential();

// create first telemetry publisher with Entra based authentication
var firstTelemetryPublisher = new HttpTelemetryPublisher
(
	httpClient,
	new Uri("INSERT INGESTION ENDPOINT 1 HERE"),
	new Guid("INSERT INSTRUMENTATION KEY 1 HERE"),
	async (cancellationToken) =>
	{
		var tokenRequestContext = new TokenRequestContext(HttpTelemetryPublisher.AuthorizationScopes);

		var token = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken);

		return new BearerToken(token.Token, token.ExpiresOn);
	}
);

// create second telemetry publisher without Entra based authentication
var secondTelemetryPublisher = new HttpTelemetryPublisher
(
	httpClient,
	new Uri("INSERT INGESTION ENDPOINT 2 HERE"),
	new Guid("INSERT INSTRUMENTATION KEY 2 HERE")
);

// create telemetry Telemetry Client
var telemetryClient = new TelemetryClient(telemetryPublishers: [firstTelemetryPublisher, secondTelemetryPublisher]);
```

### Http Dependency Tracking

The code sample below demonstrates tracking of Http request with `TelemetryTrackedHttpClientHandler` class.

```csharp
using System.Diagnostics;

using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;
using Azure.Storage.Queues;

// create an HTTP Client for telemetry publisher
using var httpClient = new HttpClient();

// create authorization token source
var tokenCredential = new DefaultAzureCredential();

// Create telemetry publisher with Entra authentication
var telemetryPublisher = new HttpTelemetryPublisher
(
	httpClient,
	new Uri("INSERT INGESTION ENDPOINT HERE"),
	new Guid("INSERT INSTRUMENTATION KEY HERE"),
	async (cancellationToken) =>
	{
		var tokenRequestContext = new TokenRequestContext(HttpTelemetryPublisher.AuthorizationScopes);
		var token = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken);
		return new BearerToken(token.Token, token.ExpiresOn);
	}
);

// create telemetry Telemetry Client
var telemetryClient = new TelemetryClient(telemetryPublishers: telemetryPublisher);

// create Http Client Handler
var handler = new TelemetryTrackedHttpClientHandler(TelemetryClient, () => ActivitySpanId.CreateRandom().ToString());

// create Http Client Transport
var queueClientHttpClientTransport = new HttpClientTransport(handler);

var queueServiceUri = new Uri("INSERT QUEUE SERVICE URI HERE");

// create Queue Client Options
var queueClientOptions = new QueueClientOptions()
{
	MessageEncoding = QueueMessageEncoding.Base64,
	Transport = queueClientHttpClientTransport
};

// create Queue Service Client
var queueService = new QueueServiceClient(queueServiceUri, tokenCredential, queueClientOptions);

// create Queue Client
var queueClient = queueService.GetQueueClient(QueueName);

// send message
_ = await queueClient.SendMessageAsync("TEST MESSAGE");

// publish collected telemetry
_ = await telemetryClient.PublishAsync();

```

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
[app_insights_create_rest]: https://learn.microsoft.com/azure/azure-monitor/app/create-workspace-resource?tabs=rest#create-an-application-insights-resource