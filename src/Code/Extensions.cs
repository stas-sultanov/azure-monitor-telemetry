// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

using System;
using System.Collections.Generic;

public static class Extensions
{
	#region Data

	private const Int32 ExceptionMaxStackLength = 32768;

	/// <summary>
	/// A dictionary mapping well-known domain names to their corresponding dependency types.
	/// </summary>
	internal static IReadOnlyDictionary<String, String> WellKnownDomainToDependencyType { get; } = new Dictionary<String, String>()
	{
	// Azure Blob
		{ ".blob.core.windows.net", DependencyType.AzureBlob },
		{ ".blob.core.chinacloudapi.cn", DependencyType.AzureBlob },
		{ ".blob.core.cloudapi.de", DependencyType.AzureBlob },
		{ ".blob.core.usgovcloudapi.net", DependencyType.AzureBlob },
	// Azure Cosmos DB
		{".documents.azure.com", DependencyType.AzureCosmosDB },
		{".documents.chinacloudapi.cn", DependencyType.AzureCosmosDB },
		{".documents.cloudapi.de", DependencyType.AzureCosmosDB },
		{".documents.usgovcloudapi.net", DependencyType.AzureCosmosDB },
	// Azure Iot
		{".azure-devices.net", DependencyType.AzureIotHub},
	// Azure Monitor
		{ ".applicationinsights.azure.com", DependencyType.AzureMonitor },
	// Azure Queue
		{ ".queue.core.windows.net", DependencyType.AzureQueue },
		{ ".queue.core.chinacloudapi.cn", DependencyType.AzureQueue },
		{ ".queue.core.cloudapi.de", DependencyType.AzureQueue },
		{ ".queue.core.usgovcloudapi.net", DependencyType.AzureQueue },
	// Azure Search
		{ ".search.windows.net", DependencyType.AzureSearch},
	// Azure Service Bus
		{".servicebus.windows.net", DependencyType.AzureServiceBus },
		{".servicebus.chinacloudapi.cn", DependencyType.AzureServiceBus },
		{".servicebus.cloudapi.de", DependencyType.AzureServiceBus },
		{".servicebus.usgovcloudapi.net", DependencyType.AzureServiceBus },
	// Azure Table
		{".table.core.windows.net", DependencyType.AzureTable},
		{".table.core.chinacloudapi.cn", DependencyType.AzureTable},
		{".table.core.cloudapi.de", DependencyType.AzureTable},
		{".table.core.usgovcloudapi.net", DependencyType.AzureTable}
	};

	#endregion

	#region Methods: Uri

	/// <summary>
	/// Detects the dependency type from the HTTP request URI.
	/// </summary>
	/// <param name="host">The host of the HTTP URI.</param>
	/// <returns>The detected dependency type, or "Http" if the host is not recognized.</returns>
	public static String DetectDependencyTypeFromHttp(this Uri uri)
	{
		var dotIndex = uri.Host.IndexOf('.');

		var domain = uri.Host.Substring(dotIndex);

		if (WellKnownDomainToDependencyType.TryGetValue(domain, out var type))
		{
			return type;
		}

		return DependencyType.HTTP;
	}

	#endregion

	#region Methods: Exception

	public static IReadOnlyList<ExceptionInfo> Convert
	(
		this Exception exception,
		Int32 maxStackLength = ExceptionMaxStackLength
	)
	{
		var result = new List<ExceptionInfo>();

		var outerId = 0;

		var currentException = exception;

		do
		{
			// get id
			var id = currentException.GetHashCode();

			// get stack trace
			var stackTrace = new System.Diagnostics.StackTrace(currentException, true);

			// get frames
			var frames = stackTrace.GetFrames();

			// calc number of frames to take
			var takeFramesCount = Math.Min(frames.Length, maxStackLength);

			var parsedStack = new StackFrameInfo[takeFramesCount];

			for (var frameIndex = 0; frameIndex < takeFramesCount; frameIndex++)
			{
				var frame = frames[frameIndex];

				var methodInfo = frame.GetMethod();

				var method = methodInfo?.DeclaringType == null ? methodInfo?.Name: String.Concat(methodInfo.DeclaringType.FullName, ".", methodInfo.Name);

				var frameInfo = new StackFrameInfo
				{
					Assembly = methodInfo?.Module.Assembly.FullName!,
					Level = frameIndex,
					Line = frame.GetFileLineNumber(),
					Method = method
				};

				parsedStack[frameIndex] = frameInfo;
			}

			var exceptionInfo = new ExceptionInfo()
			{
				HasFullStack = stackTrace.FrameCount < maxStackLength,
				Id = id,
				Message = currentException.Message.Replace("\r\n", " "),
				OuterId = outerId,
				ParsedStack = parsedStack,
				TypeName = currentException.GetType().FullName!
			};

			result.Add(exceptionInfo);

			outerId = id;

			currentException = currentException.InnerException;
		}
		while (currentException != null);

		return result;
	}

	#endregion
}
