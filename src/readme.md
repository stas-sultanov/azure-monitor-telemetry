Azure Monitor Telemetry
=======================

![NuGet Version](https://img.shields.io/nuget/v/Stas.Azure.Monitor.Telemetry)
![NuGet Downloads](https://img.shields.io/nuget/dt/Stas.Azure.Monitor.Telemetry)

A lightweight, high-performance library for tracking and publishing telemetry.

## Table of Contents
- [Getting Started](#getting-started)
	- [Prerequisites](#prerequisites)
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

It is possible to create a new **Application Insights** resource via 
[Bicep][app_insights_create_bicep],
[Azure Portal][app_insights_create_portal],
[Azure CLI][app_insights_create_cli],
[Powershell][app_insights_create_ps],
[Http Request][app_insights_create_httpapi].

### Authentication

Application Insights supports secure access via Entra based authentication, more info [here][app_insightsEntraAuth].

The Identity, on behalf of which the code will run, must be granted with the [Monitoring Metrics Publisher](https://learn.microsoft.com/azure/role-based-access-control/built-in-roles/monitor#monitoring-metrics-publisher) role.

### Initialization

The `TelemetryClient` class is the core component for tracking and publishing telemetry.

To publish telemetry to **Application Insights**, the constructor of `TelemetryClient` class must be provided with one or many instances of a class that implements `TelemetryPublisher` interface.

The library contains class `HttpTelemetryPublisher` which implements `TelemetryPublisher` interface and allows work with **Application Insights** resource via HTTP protocol.

The `TelemetryClient` class supports work with:

- An instance of **Application Insights** resource in insecure (default) way.<br/>
  This [code sample](#init-with-single-publisher) demonstrates initialization of `TelemetryClient` class with one instance of `HttpTelemetryPublisher` class.
- An instance of **Application Insights** resource in secure way via Entra authentication.<br/>
  This [code sample](#init-with-entra-auth) demonstrates initialization of `TelemetryClient` class with one instance of `HttpTelemetryPublisher` class which is configured to work with Entra authentication.
- Multiple instances of **Application Insights** resource.<br/>
  This [code sample](#init-with-multiple-publishers) demonstrates initialization of `TelemetryClient` class with two instances of `HttpTelemetryPublisher` class to send telemetry in different instances of **Application Insights** resource.<br/>
  Please note that instances of Azure Application Insights resource should be in different Azure Subscriptions.

## Work

### Adding telemetry

To add telemetry to instance of `TelemetryClient` use `TelemetryClient.Add` method.

```C#
// create telemetry item
var telemetry = new EventTelemetry(DateTime.UtcNow, @"start");

// add to the telemetryClient
telemetryClient.Add(telemetry);
```

### Tracking telemetry

The `TelemetryClient` class provides a set of *Track* methods.
The purpose of this methods is to simplify adding telemetry to the `TelemetryClient` storage.
The major point of *Track* methods is to associate telemetry item with distributed operation that is currently tracked by `TelemetryClient`.
For most of the cases Track methods will call `DateTime.UtcNow' to get current timestamp for telemetry item.

```C#
// track event and associate with current distributed operation
telemetryClient.TrackEvent(@"start");
```

### Publishing

To publish collected telemetry use `TelemetryClient.PublishAsync` method.

The collected telemetry data will be published in parallel using all configured instances of `TelemetryPublisher` interface.

```C#
// publish collected telemetry
await telemetryClient.PublishAsync(cancellationToken);
```

## Dependency Tracking

The library does not provide any automatic publishing of the data. 

This library makes use instance of `ConcurrentQueue` to collect and send telemetry data.
As a result, if the process is terminated suddenly, you could lose telemetry that is stored in the queue.
It is recommended to track the closing of your process and call the `TelemetryClient.PublishAsync()` method to ensure no telemetry is lost.

## Adding Tags
You can populate common context by using `tags` argument of the `TelemetryClient` constructor which will be automatically attached to each telemetry item sent. You can also attach additional property data to each telemetry item sent by using `Telemetry.Tags` property. The ```TelemetryClient``` exposes a method Add that adds telemetry information into the processing queue.

## Examples

### Init with Single Publisher

Example demonstrates initialization of `TelemetryClient` with one publisher.

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

// create telemetry Telemetry Client
var telemetryClient = new TelemetryClient(tags, telemetryPublishers: telemetryPublisher);
```

### Init with Entra Auth

Example demonstrates initialization of `TelemetryClient` with Entra authentication.

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

// create telemetry Telemetry Client
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

// create telemetry Telemetry Client
var telemetryClient = new TelemetryClient(telemetryPublishers: [firstTelemetryPublisher, secondTelemetryPublisher]);
```

[azure_subscription]: https://azure.microsoft.com/free/dotnet/
[azure_app_insights]: https://learn.microsoft.com/azure/templates/microsoft.insights/components
[app_insights]: https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview
[app_insights_entra_auth]: https://learn.microsoft.com/azure/azure-monitor/app/azure-ad-authentication
[app_insights_create_bicep]: https://learn.microsoft.com/azure/azure-monitor/app/create-workspace-resource?tabs=bicep#create-an-application-insights-resource
[app_insights_create_cli]: https://learn.microsoft.com/azure/azure-monitor/app/create-workspace-resource?tabs=cli#create-an-application-insights-resource
[app_insights_create_portal]: https://learn.microsoft.com/azure/azure-monitor/app/create-workspace-resource?tabs=portal#create-an-application-insights-resource
[app_insights_create_ps]: https://learn.microsoft.com/azure/azure-monitor/app/create-workspace-resource?tabs=powershell#create-an-application-insights-resource
[app_insights_create_httpapi]: https://learn.microsoft.com/azure/azure-monitor/app/create-workspace-resource?tabs=rest#create-an-application-insights-resource