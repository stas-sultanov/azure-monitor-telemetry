// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Contains well-known dependency types.
/// </summary>
/// <remarks>
/// Constants values should not be changed otherwise there will be no aggregation with data which is published by other implementations of the client.
/// </remarks>
public static class TelemetryDependencyTypes
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
}
