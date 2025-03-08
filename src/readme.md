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
		- [Prerequisites](#prerequisites-1)
		- [Initialization](#initialization)
		- [Tracking](#tracking)
		- [Publishing](#publishing)
- [Dependency Tracking](#dependency-tracking)
- [Extensibility](#extensibility)
	- [Adding Tags](#adding-tags)
		- [TelemetryPublisher](#telemetrypublisher)
	- [Examples](#examples)
		- [Init with Single Publisher](#init-with-single-publisher)
		- [Init with Entra Auth](#init-with-entra-auth)
		- [Init with Multiple Publishers](#init-with-multiple-publishers)

## Getting Started

The library is designed to work with the Azure resource of [Microsoft.Insights/components][AzureInsightsComponentsResource] type aka [Application Insights][app_insights]. 

### Prerequisites

To use the library an [Azure subscription][azure_subscription] and an [Application Insights][app_insights] resource are required.

It is possible to create a new **Application Insights** resource via:
- [Azure Portal][storage_account_create_portal],
- 
[Azure PowerShell][storage_account_create_ps], or the [Azure CLI][storage_account_create_cli].
Here's an example using the Azure CLI:

```Powershell
az storage account create --name MyStorageAccount --resource-group MyResourceGroup --location westus --sku Standard_LRS
```

### Prerequisites
To use this library, required:
- An instance of **Application Insights** in the same region as services for optimal performance and cost efficiency.
- An **Ingestion Endpoint and** an **Instrumentation Key**, available in the **Connection String** property of **Application Insights** resource.

### Initialization

The `TelemetryClient` class is the core component for tracking and publishing telemetry.  

To publish telemetry to **Application Insights**, the constructor of `TelemetryPublisher` must be provided with an instance of a class that implements `TelemetryPublisher` interface.

Application Insights supports secure access via Entra based authentication, more info [here][AppInsightsEntraAuth].

The Identity, on behalf of which the code will run, must be granted with the [Monitoring Metrics Publisher](https://learn.microsoft.com/azure/role-based-access-control/built-in-roles/monitor#monitoring-metrics-publisher) role.

Code sample below demonstrates


The library supports multiple telemetry publishers, enabling collected telemetry to be published to multiple **Application Insights** instances.

The library includes `HttpTelemetryPublisher`, the default implementation of `TelemetryPublisher` interface. 


### Tracking

To add telemetry to instance of `TelemetryClient` use `TelemetryClient.Add` method.

```C#
// create telemetry item
var telemetry = new EventTelemetry(DateTime.UtcNow, @"start");

// add to the telemetryClient
telemetryClient.Add(telemetry);
```

### Publishing

To publish collected telemetry use `TelemetryClient.PublishAsync` method.

The collected telemetry data will be published in parallel using all configured instances of `TelemetryPublisher` interface.

```C#
// publish collected telemetry
await telemetryClient.PublishAsync(cancellationToken);
```

# Dependency Tracking

The library does not provide any automatic publishing of the data. 

This library makes use instance of `ConcurrentQueue` to collect and send telemetry data.
As a result, if the process is terminated suddenly, you could lose telemetry that is stored in the queue.
It is recommended to track the closing of your process and call the `TelemetryClient.PublishAsync()` method to ensure no telemetry is lost.


# Extensibility

The library provides several points of potential extensibility.

## Adding Tags
You can populate common context by using `tags` argument of the `TelemetryClient` constructor which will be automatically attached to each telemetry item sent. You can also attach additional property data to each telemetry item sent by using `Telemetry.Tags` property. The ```TelemetryClient``` exposes a method Add that adds telemetry information into the processing queue.

### TelemetryPublisher
If needed it is possible to implement own 

## Examples

### Init with Single Publisher

Example demonstrates initialization of `TelemetryClient`with one publisher.

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

// create telemetry telemetryClient
var telemetryClient = new TelemetryClient(tags, telemetryPublishers: telemetryPublisher);
```

### Init with Entra Auth

 initialization of TelemetryClient with Entra based authentication.

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

// create telemetry telemetryClient
var telemetryClient = new TelemetryClient(telemetryPublishers: telemetryPublisher);
```

### Init with Multiple Publishers

The code sample below demonstrates initialization of the `TelemetryClient` for the scenario
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

// create telemetry telemetryClient
var telemetryClient = new TelemetryClient(telemetryPublishers: [firstTelemetryPublisher, secondTelemetryPublisher]);
```

[azure_subscription]: https://azure.microsoft.com/free/dotnet/
[AppInsights]: https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview
[AppInsightsEntraAuth]: https://learn.microsoft.com/azure/azure-monitor/app/azure-ad-authentication
[AzureCLI]: https://learn.microsoft.com/cli/azure/
[AzureInsightsComponentsResource]: https://learn.microsoft.com/azure/templates/microsoft.insights/components
[AzurePortal]: https://portal.azure.com


[app_insights_create_cli]: https://learn.microsoft.com/en-us/azure/azure-monitor/app/create-workspace-resource?tabs=cli
[app_insights_create_portal]: https://learn.microsoft.com/en-us/azure/azure-monitor/app/create-workspace-resource?tabs=portal
[app_insights_create_ps]: https://learn.microsoft.com/en-us/azure/azure-monitor/app/create-workspace-resource?tabs=powershell
[app_insights_create_rest]: https://learn.microsoft.com/en-us/azure/azure-monitor/app/create-workspace-resource?tabs=rest