# Azure Monitor Telemetry

![NuGet Version](https://img.shields.io/nuget/v/Stas.Azure.Monitor.Telemetry)
![NuGet Downloads](https://img.shields.io/nuget/dt/Stas.Azure.Monitor.Telemetry)

A lightweight, high-performance library for tracking application telemetry with Azure Monitor.

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

### Authentication

**Application Insights** supports secure access via [Entra authentication][app_insights_entra_auth].

The identity running the code must be granted with the [Monitoring Metrics Publisher][azure_rbac_monitoring_metrics_publisher] role.

The authentication token must have `https://monitor.azure.com//.default` as its audience.

### Initialization

The `TelemetryClient` class is the core component for tracking and publishing telemetry.

To publish telemetry to **Application Insights**, the constructor of the `TelemetryClient` class must be provided with one or more instances of a class that implements the `TelemetryPublisher` interface.

The library provides the `HttpTelemetryPublisher` class, which implements the `TelemetryPublisher` interface and allows working with the **Application Insights** resource via the HTTP protocol.

The `TelemetryClient` class supports working with:

- An instance of **Application Insights** resource in an insecure (default) way.
  This [code sample](#init-with-single-publisher) demonstrates the initialization of the `TelemetryClient` class with one instance of the `HttpTelemetryPublisher` class.
- An instance of **Application Insights** resource in a secure way via Entra authentication.
  This [code sample](#init-with-entra-auth) demonstrates the initialization of the `TelemetryClient` class with one instance of the `HttpTelemetryPublisher` class configured to work with Entra authentication.
- Multiple instances of **Application Insights** resource.
  This [code sample](#http-dependency-tracking) demonstrates the initialization of the `TelemetryClient` class with two instances of the `HttpTelemetryPublisher` class to send telemetry to different instances of the **Application Insights** resource.

## Supported Telemetry Types

The library provides support for all telemetry types supported by **Application Insights**.

There are two types of telemetry:
1. Those which represent information at a specific timestamp:
    1. [EventTelemetry](/src/Code/Models/EventTelemetry.cs)
    1. [ExceptionTelemetry](/src/Code/Models/ExceptionTelemetry.cs)
    1. [MetricTelemetry](/src/Code/Models/MetricTelemetry.cs)
    1. [TraceTelemetry](/src/Code/Models/TraceTelemetry.cs)
1. Those which represent an acivity with a start timestamp and duration:
    1. [AvailabilityTelemetry](/src/Code/Models/AvailabilityTelemetry.cs)
    1. [DependencyTelemetry](/src/Code/Models/DependencyTelemetry.cs)
    1. [PageViewTelemetry](/src/Code/Models/PageViewTelemetry.cs)
    1. [RequestTelemetry](/src/Code/Models/RequestTelemetry.cs)

## Adding Telemetry

The `TelemetryClient` class provides a method `Add` that adds an instance of a class that implements the `Telemetry` interface into the processing queue.

```csharp
// create telemetry item
var telemetry = new EventTelemetry(DateTime.UtcNow, "start");

// add to the telemetryClient
telemetryClient.Add(telemetry);
```

### Tracking Telemetry

The `TelemetryClient` class provides a set of `Track` methods.
The purpose of these methods is to simplify adding telemetry to the `TelemetryClient` storage.
The major point of `Track` methods is to associate telemetry items with the distributed operation that is currently tracked by `TelemetryClient`.
For most cases, Track methods will call `DateTime.UtcNow` to get the current timestamp for the telemetry item.

```csharp
// track event and associate with current distributed operation
telemetryClient.TrackEvent("start");
```

## Dependency Tracking

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

Telemetry tags is a mechanism of enriching telemetry information with specific data like CloudRole of the machine which executes the code.

The list of standard tags can be found in [TelemetryTagKeys](/src/Code/TelemetryTagKeys.cs) class.

There ara verity of ways you may add tags to the telemetry:
1. The `Telemetry` interface provides a property `Tags` that allows to attach tags to specific telemetry item.
1. The `TelemetryClient` class constructor accepts an argument 'tags'.
   In this case provided tags will be automatically attached to each telemetry item published via each `TelemetryPublisher` during the publish operation.
1. The `HttpTelemetryPublisher` class constructor accepts an argument 'tags'.
   In this case provided tags will be automatically attached to each telemetry item during publish operation.

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