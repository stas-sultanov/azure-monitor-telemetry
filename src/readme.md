Azure Monitor Telemetry
=======================

![NuGet Version](https://img.shields.io/nuget/v/Stas.Azure.Monitor.Telemetry)
![NuGet Downloads](https://img.shields.io/nuget/dt/Stas.Azure.Monitor.Telemetry)

A lightweight, high-performance library for tracking and publishing telemetry.

## Table of Contents  
- [Azure Monitor Telemetry](#azure-monitor-telemetry)
	- [Table of Contents](#table-of-contents)
	- [Getting Started](#getting-started)
		- [Prerequisites](#prerequisites)
		- [Initialization](#initialization)
			- [Single Publisher](#single-publisher)
			- [Single Publisher With Entra Authentication](#single-publisher-with-entra-authentication)
			- [Multiple Publishers](#multiple-publishers)
		- [Tracking](#tracking)
		- [Publishing](#publishing)
- [Dependency Tracking](#dependency-tracking)
- [Extensibility](#extensibility)
	- [Adding Tags](#adding-tags)
		- [TelemetryPublisher](#telemetrypublisher)

## Getting Started

The library is designed to work with the Azure resource of [Microsoft.Insights/components][AzureInsightsComponentsResource] type aka [Application Insights][AppInsights]. 

### Prerequisites
To use this library, required:
- An instance of **Application Insights** in the same region as services for optimal performance and cost efficiency.
- An **Ingestion Endpoint and** an **Instrumentation Key**, available in the **Connection String** property of **Application Insights** resource.

### Initialization

The `TelemetryTracker` class is the core component for tracking and publishing telemetry.  

To publish telemetry to **Application Insights**, the constructor of `TelemetryPublisher` must be provided with an instance of a class that implements `TelemetryPublisher` interface.

The library supports multiple telemetry publishers, enabling collected telemetry to be published to multiple **Application Insights** instances.

The library includes `HttpTelemetryPublisher`, the default implementation of `TelemetryPublisher` interface. 

#### Single Publisher

Example demonstrates initialization with a single publisher:  

```C#
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

// create telemetry tracker
var telemetryTracker = new TelemetryTracker(tags, telemetryPublishers: telemetryPublisher);
```

#### Single Publisher With Entra Authentication

Application Insights supports secure access via Entra based authentication, more info [here][AppInsightsEntraAuth].

The Identity, on behalf of which the code will run, must be granted with the [Monitoring Metrics Publisher](https://learn.microsoft.com/azure/role-based-access-control/built-in-roles/monitor#monitoring-metrics-publisher) role.

Code sample below demonstrates initialization of TelemetryTracker with Entra based authentication.

```C#
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

// create telemetry tracker
var telemetryTracker = new TelemetryTracker(telemetryPublishers: telemetryPublisher);
```

#### Multiple Publishers

The code sample below demonstrates initialization of the `TelemetryTracker` for the scenario
where it is required to publish telemetry data into multiple instances of **Application Insights**.

```C#
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

// create telemetry tracker
var telemetryTracker = new TelemetryTracker(telemetryPublishers: [firstTelemetryPublisher, secondTelemetryPublisher]);
```

### Tracking

To add telemetry to instance of `TelemetryTracker` use `TelemetryTracker.Add` method.

```C#
// create telemetry item
var telemetry = new EventTelemetry(DateTime.UtcNow, @"start");

// add to the tracker
telemetryTracker.Add(telemetry);
```

### Publishing

To publish collected telemetry use `TelemetryTracker.PublishAsync` method.

The collected telemetry data will be published in parallel using all configured instances of `TelemetryPublisher` interface.

```C#
// publish collected telemetry
await telemetryTracker.PublishAsync(cancellationToken);
```

# Dependency Tracking

The library does not provide any automatic publishing of the data. 

This library makes use instance of `ConcurrentQueue` to collect and send telemetry data.
As a result, if the process is terminated suddenly, you could lose telemetry that is stored in the queue.
It is recommended to track the closing of your process and call the `TelemetryTracker.PublishAsync()` method to ensure no telemetry is lost.


# Extensibility

The library provides several points of potential extensibility.

## Adding Tags
You can populate common context by using `tags` argument of the `TelemetryTracker` constructor which will be automatically attached to each telemetry item sent. You can also attach additional property data to each telemetry item sent by using `Telemetry.Tags` property. The ```TelemetryClient``` exposes a method Add that adds telemetry information into the processing queue.

### TelemetryPublisher
If needed it is possible to implement own 

[AppInsights]: https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview
[AppInsightsEntraAuth]: https://learn.microsoft.com/azure/azure-monitor/app/azure-ad-authentication
[AzureCLI]: https://learn.microsoft.com/cli/azure/
[AzureInsightsComponentsResource]: https://learn.microsoft.com/azure/templates/microsoft.insights/components
[AzurePortal]: https://portal.azure.com
