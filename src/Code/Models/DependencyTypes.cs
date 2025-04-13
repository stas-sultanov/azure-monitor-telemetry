// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Models;

/// <summary>
/// Contains well-known dependency types.
/// </summary>
public static class DependencyTypes
{
	#region Constants

	/// <summary>
	/// The Application Insights HTTP tracked component.
	/// </summary>
	public const String AI = "Http (tracked component)";

	/// <summary>
	/// The Azure Blob service.
	/// </summary>
	public const String AzureBlob = "Azure blob";

	/// <summary>
	/// The Azure Cosmos DB service.
	/// </summary>
	public const String AzureCosmosDB = "Azure DocumentDB";

	/// <summary>
	/// The Azure Event Hubs service.
	/// </summary>
	public const String AzureEventHubs = "Azure Event Hubs";

	/// <summary>
	/// The Azure IoT Hub service.
	/// </summary>
	public const String AzureIotHub = "Azure IoT Hub";

	/// <summary>
	/// The Azure Monitor service.
	/// </summary>
	public const String AzureMonitor = "Azure Monitor";

	/// <summary>
	/// The Azure Queue service.
	/// </summary>
	public const String AzureQueue = "Azure queue";

	/// <summary>
	/// The Azure Search service.
	/// </summary>
	public const String AzureSearch = "Azure Search";

	/// <summary>
	/// The Azure Service Bus service.
	/// </summary>
	public const String AzureServiceBus = "Azure Service Bus";

	/// <summary>
	/// The Azure Table service.
	/// </summary>
	public const String AzureTable = "Azure table";

	/// <summary>
	/// The generic HTTP service.
	/// </summary>
	public const String HTTP = "Http";

	/// <summary>
	/// The in-process.
	/// </summary>
	public const String InProc = "InProc";

	/// <summary>
	/// The queue message.
	/// </summary>
	public const String QueueMessage = "Queue Message";

	/// <summary>
	/// The SQL database.
	/// </summary>
	public const String SQL = "SQL";

	/// <summary>
	/// The WCF service.
	/// </summary>
	public const String WcfService = "WCF Service";

	/// <summary>
	/// The web service.
	/// </summary>
	public const String WebService = "Web Service";

	#endregion
}
