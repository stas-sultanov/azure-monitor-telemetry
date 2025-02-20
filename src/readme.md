# Azure Monitor Telemetry

![NuGet Version](https://img.shields.io/nuget/v/Stas.Azure.Monitor.Telemetry)
![NuGet Downloads](https://img.shields.io/nuget/dt/Stas.Azure.Monitor.Telemetry)

A lightweight, high-performance library for tracking and publishing telemetry.

## Getting Started

### Get an Ingestion Endpoint and an Instrumentation Key

To use the library you will need to provide it with an Ingestion Endpoint and an Instrumentation Key which can be obtained from the ConnectionString property of the Azure resource of [Microsoft.Insights/components][AzureInsightsComponentsResource] type aka Application Insights. 

### Initialize a TelemetryTracker

The **TelemetryTracker** is the primary root object for the library.
All functionality around telemetry tracking and publishing is located on this object.

To initialize instance of **TelemetryTracker** type you should provide an instance of the type that implements **TelemetryPublisher** interface.

**HttpTelemetryPublisher** type implements **TelemetryPublisher** interface and provides an ability to publish telemetry data via HTTP protocol both with and without Entra based authentication.

It is possible to configure **TelemetryTracker** to publish telemetry
**TelemetryTracker** accepts array of instances of of the type that implements **TelemetryPublisher** interface, in this way it is possible to configure

#### One Publisher without Entra based Authentication

Constructor of `TelemetryTracker` type requires to provide an Ingestion Endpoint Uri and Instrumentation Key to identify your application.

The code sample below demonstrates initialization of `TelemetryTracker` to work with the instance of Azure resource of [Microsoft.Insights/components][AzureInsightsComponentsResource] type which does not require Entra authentication.

<details>
  <summary>Code sample.</summary>

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

// create telemetry tracker
var telemetryTracker = new TelemetryTracker(telemetryPublishers: telemetryPublisher);
```
</details>

#### One Publisher with Entra based Authentication

The code sample below demonstrates initialization of `TelemetryTracker` to work with the instance of Azure resource of [Microsoft.Insights/components][AzureInsightsComponentsResource] type which require Entra authentication.

In this scenario the Entra Identity, on behalf of which the code will run, must be granted with the [Monitoring Metrics Publisher](https://learn.microsoft.com/azure/role-based-access-control/built-in-roles/monitor#monitoring-metrics-publisher) role.

<details>
  <summary>Code sample.</summary>

```C#
using Azure.Identity;
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;

// create an HTTP Client for telemetry publisher
using var httpClient = new HttpClient();

// create authorization token source
var tokenCredential = new DefaultAzureCredential();

// create telemetry publisher
var telemetryPublisher = new HttpTelemetryPublisher
(
	telemetrySenderHttpClient,
	new Uri("INSERT INGESTION ENDPOINT HERE"),
	new Guid("INSERT INSTRUMENTATION KEY HERE"),
	async (cancellationToken) =>
	{
		var tokenRequestContext = new TokenRequestContext(HttpTelemetrySender.AuthorizationScopes);

		var token = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken);

		return new BearerToken(token.Token, token.ExpiresOn);
	}
);

// create telemetry tracker
var telemetryTracker = new TelemetryTracker(telemetryPublishers: telemetryPublisher);
```
</details>

#### Many publishers

The library supports publishing of telemetry data to many Azure resources of [Microsoft.Insights/components][AzureInsightsComponentsResource] type.

The code sample below demonstrates initialization of the `TelemetryTracker` for the scenario where it is required to publish data to two different Azure resources of [Microsoft.Insights/components][AzureInsightsComponentsResource] type.
The first one uses Entra based authentication the second does not.

<details>
  <summary>Code sample.</summary>

```C#
using Azure.Identity;
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;

// create an HTTP Client for telemetry publisher
using var httpClient = new HttpClient();

// create authorization token source
var tokenCredential = new DefaultAzureCredential();

// create first telemetry publisher
var firstTelemetryPublisher = new HttpTelemetryPublisher
(
	telemetrySenderHttpClient,
	new Uri("INSERT HERE: INGESTION ENDPOINT FOR FIRST"),
	new Guid("INSERT HERE: INSTRUMENTATION FOR FIRST"),
	async (cancellationToken) =>
	{
		var tokenRequestContext = new TokenRequestContext(HttpTelemetrySender.AuthorizationScopes);

		var token = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken);

		return new BearerToken(token.Token, token.ExpiresOn);
	}
);

// create second telemetry publisher
var secondTelemetryPublisher = new HttpTelemetryPublisher
(
	httpClient,
	new Uri("INSERT INGESTION ENDPOINT HERE"),
	new Guid("INSERT INSTRUMENTATION KEY HERE")
);

// create telemetry tracker
var telemetryTracker = new TelemetryTracker(telemetryPublishers: firstTelemetryPublisher, secondTelemetryPublisher);
```
</details>

### Use the TelemetryTracker to track telemetry

This library does not provide any automatic telemetry collection.
You can populate common context by using `tags` argument of the `TelemetryTracker` constructor which will be automatically attached to each telemetry item sent. You can also attach additional property data to each telemetry item sent by using `Telemetry.Tags` property. The `TelemetryClient` exposes a method Add that adds telemetry information into the processing queue.

```C#


``` 

## Publishing Telemetry data

To publish telemetry data to all configured instance of `TelemetryPublisher` type, you should call `TelemetryTracker.PublishAsync` method.

<details>
  <summary>Code sample.</summary>

```C#

// create telemetry tracker
await telemetryTracker.PublishAsync(cancellationToken);

```
</details>

The library does not provide any automatic publishing of the data. 

This library makes use instance of `ConcurrentQueue` to collect and send telemetry data.
As a result, if the process is terminated suddenly, you could lose telemetry that is stored in the queue.
It is recommended to track the closing of your process and call the `TelemetryTracker.PublishAsync()` method to ensure no telemetry is lost.


[AzureInsightsComponentsResource]: https://learn.microsoft.com/azure/templates/microsoft.insights/components