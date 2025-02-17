// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Provides constants and methods for identifying dependency types.
/// </summary>
/// <remarks>
/// Constants values should not be changed otherwise there will be no aggregation with data which is published by other implementations of the client.
/// </remarks>
public static class DependencyType
{
	#region Constants

	/// <summary>
	/// Represents an Application Insights HTTP tracked component.
	/// </summary>
	public const String AI = "Http (tracked component)";

	/// <summary>
	/// Represents an Azure Blob storage dependency.
	/// </summary>
	public const String AzureBlob = "Azure blob";

	/// <summary>
	/// Represents an Azure Cosmos DB dependency.
	/// </summary>
	public const String AzureCosmosDB = "Azure DocumentDB";

	/// <summary>
	/// Represents an Azure Event Hubs dependency.
	/// </summary>
	public const String AzureEventHubs = "Azure Event Hubs";

	/// <summary>
	/// Represents an Azure IoT Hub dependency.
	/// </summary>
	public const String AzureIotHub = "Azure IoT Hub";

	/// <summary>
	/// Represents an Azure Monitor dependency.
	/// </summary>
	public const String AzureMonitor = "Azure Monitor";

	/// <summary>
	/// Represents an Azure Queue storage dependency.
	/// </summary>
	public const String AzureQueue = "Azure queue";

	/// <summary>
	/// Represents an Azure Search dependency.
	/// </summary>
	public const String AzureSearch = "Azure Search";

	/// <summary>
	/// Represents an Azure Service Bus dependency.
	/// </summary>
	public const String AzureServiceBus = "Azure Service Bus";

	/// <summary>
	/// Represents an Azure Table storage dependency.
	/// </summary>
	public const String AzureTable = "Azure table";

	/// <summary>
	/// Represents an HTTP dependency.
	/// </summary>
	public const String HTTP = "Http";

	/// <summary>
	/// Represents an in-process dependency.
	/// </summary>
	public const String InProc = "InProc";

	/// <summary>
	/// Represents a queue message dependency.
	/// </summary>
	public const String QueueMessage = "Queue Message";

	/// <summary>
	/// Represents a SQL database dependency.
	/// </summary>
	public const String SQL = "SQL";

	/// <summary>
	/// Represents a WCF service dependency.
	/// </summary>
	public const String WcfService = "WCF Service";

	/// <summary>
	/// Represents a web service dependency.
	/// </summary>
	public const String WebService = "Web Service";

	#endregion

	#region Methods

	/// <summary>
	/// A dictionary mapping well-known domain names to their corresponding dependency types.
	/// </summary>
	public static IReadOnlyDictionary<String, String> WellKnownDomainToDependencyType { get; } = new Dictionary<String, String>()
	{
	// Azure Blob
		{ ".blob.core.windows.net", AzureBlob },
		{ ".blob.core.chinacloudapi.cn", AzureBlob },
		{ ".blob.core.cloudapi.de", AzureBlob },
		{ ".blob.core.usgovcloudapi.net", AzureBlob },
	// Azure Cosmos DB
		{".documents.azure.com", AzureCosmosDB },
		{".documents.chinacloudapi.cn", AzureCosmosDB },
		{".documents.cloudapi.de", AzureCosmosDB },
		{".documents.usgovcloudapi.net", AzureCosmosDB },
	// Azure Iot
		{".azure-devices.net", AzureIotHub},
	// Azure Monitor
		{ ".applicationinsights.azure.com", AzureMonitor },
	// Azure Queue
		{ ".queue.core.windows.net", AzureQueue },
		{ ".queue.core.chinacloudapi.cn", AzureQueue },
		{ ".queue.core.cloudapi.de", AzureQueue },
		{ ".queue.core.usgovcloudapi.net", AzureQueue },
	// Azure Search
		{ ".search.windows.net", AzureSearch},
	// Azure Service Bus
		{".servicebus.windows.net", AzureServiceBus },
		{".servicebus.chinacloudapi.cn", AzureServiceBus },
		{".servicebus.cloudapi.de", AzureServiceBus },
		{".servicebus.usgovcloudapi.net", AzureServiceBus },
	// Azure Table
		{".table.core.windows.net", AzureTable},
		{".table.core.chinacloudapi.cn", AzureTable},
		{".table.core.cloudapi.de", AzureTable},
		{".table.core.usgovcloudapi.net", AzureTable}
	};

	/// <summary>
	/// Detects the dependency type from the HTTP request URI.
	/// </summary>
	/// <param name="host">The host of the HTTP URI.</param>
	/// <returns>The detected dependency type, or "Http" if the host is not recognized.</returns>
	public static String DetectTypeFromHttp(Uri uri)
	{
		var dotIndex = uri.Host.IndexOf('.');

		var domain = uri.Host.Substring(dotIndex);

		if (WellKnownDomainToDependencyType.TryGetValue(domain, out var type))
		{
			return type;
		}

		return HTTP;
	}

	#endregion
}
