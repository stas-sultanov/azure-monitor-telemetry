// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides a set of extension methods.
/// </summary>
public static class Extensions
{
	#region Data

	private const Int32 ExceptionMaxStackLength = 32768;
	private const Int32 ExceptionMaxMessageLength = 32768;

	/// <summary>
	/// A dictionary mapping well-known domain names to their corresponding dependency types.
	/// </summary>
	internal static IReadOnlyDictionary<String, String> WellKnownDomainToDependencyType { get; } = new Dictionary<String, String>()
	{
	// Azure Blob
		{ ".blob.core.windows.net", TelemetryDependencyTypes.AzureBlob },
		{ ".blob.core.chinacloudapi.cn", TelemetryDependencyTypes.AzureBlob },
		{ ".blob.core.cloudapi.de", TelemetryDependencyTypes.AzureBlob },
		{ ".blob.core.usgovcloudapi.net", TelemetryDependencyTypes.AzureBlob },
	// Azure Cosmos DB
		{".documents.azure.com", TelemetryDependencyTypes.AzureCosmosDB },
		{".documents.chinacloudapi.cn", TelemetryDependencyTypes.AzureCosmosDB },
		{".documents.cloudapi.de", TelemetryDependencyTypes.AzureCosmosDB },
		{".documents.usgovcloudapi.net", TelemetryDependencyTypes.AzureCosmosDB },
	// Azure Iot
		{".azure-devices.net", TelemetryDependencyTypes.AzureIotHub},
	// Azure Monitor
		{ ".applicationinsights.azure.com", TelemetryDependencyTypes.AzureMonitor },
	// Azure Queue
		{ ".queue.core.windows.net", TelemetryDependencyTypes.AzureQueue },
		{ ".queue.core.chinacloudapi.cn", TelemetryDependencyTypes.AzureQueue },
		{ ".queue.core.cloudapi.de", TelemetryDependencyTypes.AzureQueue },
		{ ".queue.core.usgovcloudapi.net", TelemetryDependencyTypes.AzureQueue },
	// Azure Search
		{ ".search.windows.net", TelemetryDependencyTypes.AzureSearch},
	// Azure Service Bus
		{".servicebus.windows.net", TelemetryDependencyTypes.AzureServiceBus },
		{".servicebus.chinacloudapi.cn", TelemetryDependencyTypes.AzureServiceBus },
		{".servicebus.cloudapi.de", TelemetryDependencyTypes.AzureServiceBus },
		{".servicebus.usgovcloudapi.net", TelemetryDependencyTypes.AzureServiceBus },
	// Azure Table
		{".table.core.windows.net", TelemetryDependencyTypes.AzureTable},
		{".table.core.chinacloudapi.cn", TelemetryDependencyTypes.AzureTable},
		{".table.core.cloudapi.de", TelemetryDependencyTypes.AzureTable},
		{".table.core.usgovcloudapi.net", TelemetryDependencyTypes.AzureTable}
	};

	#endregion

	#region Methods: Uri

	/// <summary>
	/// Detects the dependency type from the HTTP request URI.
	/// </summary>
	/// <param name="uri">The HTTP URI.</param>
	/// <returns>The detected dependency type, or "Http" if the host is not recognized.</returns>
	public static String DetectDependencyTypeFromHttp(this Uri uri)
	{
		var dotIndex = uri.Host.IndexOf('.');

		var domain = uri.Host.Substring(dotIndex);

		if (WellKnownDomainToDependencyType.TryGetValue(domain, out var type))
		{
			return type;
		}

		return TelemetryDependencyTypes.HTTP;
	}

	#endregion

	#region Methods: Exception

	/// <summary>
	/// Converts <paramref name="exception"/> to read-only list of items of <see cref="ExceptionInfo"/> type.
	/// </summary>
	/// <param name="exception">The exception to convert.</param>
	/// <param name="maxStackLength">Maximal number of items to put into the <see cref="ExceptionInfo.ParsedStack"/>.</param>
	/// <returns>A read-only list of items of  <see cref="ExceptionInfo"/> type.</returns>
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

			// get message
			var message = currentException.Message.Replace("\r\n", " ");

			if (message.Length > ExceptionMaxMessageLength)
			{
				// adjust message
				message = message.Substring(0, ExceptionMaxMessageLength);
			}

			StackFrameInfo[]? parsedStack;

			// get frames
			var frames = stackTrace.GetFrames();

			if (frames == null || frames.Length == 0)
			{
				parsedStack = null;
			}
			else
			{
				// calc number of frames to take
				var takeFramesCount = Math.Min(frames.Length, maxStackLength);

				parsedStack = new StackFrameInfo[takeFramesCount];

				for (var frameIndex = 0; frameIndex < takeFramesCount; frameIndex++)
				{
					var frame = frames[frameIndex];

					var methodInfo = frame.GetMethod();

					var method = methodInfo?.DeclaringType == null ? methodInfo?.Name: String.Concat(methodInfo.DeclaringType.FullName, ".", methodInfo.Name);

					var line = frame.GetFileLineNumber();

					if (line is > (-1000000) and < 1000000)
					{
						line = 0;
					}

					var fileName = frame.GetFileName()?.Replace(@"\", @"\\");

					var frameInfo = new StackFrameInfo
					{
						Assembly = methodInfo?.Module.Assembly.FullName!,
						FileName = fileName,
						Level = frameIndex,
						Line = line,
						Method = method
					};

					parsedStack[frameIndex] = frameInfo;
				}
			}

			var exceptionInfo = new ExceptionInfo()
			{
				HasFullStack = stackTrace.FrameCount < maxStackLength,
				Id = id,
				Message = message,
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
