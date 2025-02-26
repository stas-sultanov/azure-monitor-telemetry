// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Publish;

using System.IO;
using System.Runtime.CompilerServices;

using Azure.Monitor.Telemetry;

/// <summary>
/// Provides serialization of types that implements <see cref="Telemetry"/> into the stream using JSON format.
/// </summary>
/// <remarks>Uses version 2 of the HTTP API of the Azure Monitor service.</remarks>
public static class JsonTelemetrySerializer
{
	#region Constants

	private const Int32 ExceptionMaxStackLength = 32768;
	private const String Name_Availability = @"AppAvailabilityResults";
	private const String Name_Dependency = @"AppDependencies";
	private const String Name_Event = @"AppEvents";
	private const String Name_Exception = @"AppExceptions";
	private const String Name_Metric = @"AppMetrics";
	private const String Name_PageView = @"AppPageViews";
	private const String Name_Request = @"AppRequests";
	private const String Name_Trace = @"AppTraces";
	private const String Type_Availability = "AvailabilityData";
	private const String Type_Dependency = @"RemoteDependencyData";
	private const String Type_Event = @"EventData";
	private const String Type_Exception = @"ExceptionData";
	private const String Type_Metric = @"MetricData";
	private const String Type_PageView = @"PageViewData";
	private const String Type_Request = @"RequestData";
	private const String Type_Trace = @"MessageData";

	#endregion

	#region Data

	/// <summary>
	/// <see cref="SeverityLevel"/> to <see cref="String"/> mapping.
	/// </summary>
	private static readonly String[] severityLevelToString = [@"Verbose", @"Information", @"Warning", @"Error", @"Critical"];

	#endregion

	#region Methods: Serialize

	/// <summary>
	/// Serializes the given telemetry data to JSON format and writes it to the <paramref name="streamWriter"/>.
	/// </summary>
	/// <param name="streamWriter">The StreamWriter to which the serialized JSON will be written.</param>
	/// <param name="instrumentationKey">The instrumentation key associated with the telemetry data.</param>
	/// <param name="telemetry">The telemetry data to be serialized.</param>
	/// <param name="trackerTags">A list of tags to attach to each telemetry item. From <see cref="TelemetryTracker"/>.</param>
	/// <param name="publisherTags">A list of tags to attach to each telemetry item. From <see cref="HttpTelemetryPublisher"/>.</param>
	public static void Serialize
	(
		StreamWriter streamWriter,
		String instrumentationKey,
		Telemetry telemetry,
		IReadOnlyList<KeyValuePair<String, String>>? trackerTags,
		IReadOnlyList<KeyValuePair<String, String>>? publisherTags
	)
	{
		String name;

		String baseType;

		Action<StreamWriter, Telemetry> writeData;

		// MS Engineers are not familiar with the term "CONSISTENCY" and it's meaning.
		// For TelemetryMetric, Properties must be in other place of the data structure...
		Boolean writeProperties;

		switch (telemetry)
		{
			case AvailabilityTelemetry:
				name = Name_Availability;
				baseType = Type_Availability;
				writeData = WriteDataAvailability;
				writeProperties = true;
				break;
			case DependencyTelemetry:
				name = Name_Dependency;
				baseType = Type_Dependency;
				writeData = WriteDataDependency;
				writeProperties = true;
				break;
			case EventTelemetry:
				name = Name_Event;
				baseType = Type_Event;
				writeData = WriteDataEvent;
				writeProperties = true;
				break;
			case ExceptionTelemetry:
				name = Name_Exception;
				baseType = Type_Exception;
				writeData = WriteDataException;
				writeProperties = true;
				break;
			case MetricTelemetry:
				name = Name_Metric;
				baseType = Type_Metric;
				writeData = WriteDataMetric;
				writeProperties = false;
				break;
			case PageViewTelemetry:
				name = Name_PageView;
				baseType = Type_PageView;
				writeData = WriteDataPageView;
				writeProperties = true;
				break;
			case RequestTelemetry:
				name = Name_Request;
				baseType = Type_Request;
				writeData = WriteDataRequest;
				writeProperties = true;
				break;
			case TraceTelemetry:
				name = Name_Trace;
				baseType = Type_Trace;
				writeData = WriteDataTrace;
				writeProperties = true;
				break;
			default:
				return;
		}

		streamWriter.Write("{\"data\":{\"baseData\":{");

		writeData(streamWriter, telemetry);

		streamWriter.Write(",\"ver\":2},\"baseType\":\"");

		streamWriter.Write(baseType);

		streamWriter.Write("\"},\"iKey\":\"");

		streamWriter.Write(instrumentationKey);

		streamWriter.Write("\",\"name\":\"");

		streamWriter.Write(name);

		streamWriter.Write("\"");

		// serialize properties
		if (writeProperties && telemetry.Properties != null && telemetry.Properties.Count != 0)
		{
			streamWriter.Write(",\"properties\":{");

			_ = WriteList(streamWriter, telemetry.Properties, false);

			streamWriter.Write("}");
		}

		streamWriter.Write(",\"tags\":{");

		WriteTags(streamWriter, telemetry.Operation, telemetry.Tags, trackerTags, publisherTags);

		streamWriter.Write("},\"time\":\"");

		streamWriter.Write(telemetry.Time.ToString("O"));

		streamWriter.Write("\"}");
	}

	#endregion

	#region Methods: Write Data

	private static void WriteDataAvailability(StreamWriter streamWriter, Telemetry telemetry)
	{
		var availabilityTelemetry = (AvailabilityTelemetry) telemetry;

		var success = availabilityTelemetry.Success ? "true" : "false";

		streamWriter.Write("\"duration\":\"");

		streamWriter.Write(availabilityTelemetry.Duration);

		streamWriter.Write("\",\"id\":\"");

		streamWriter.Write(availabilityTelemetry.Id);

		streamWriter.Write("\"");

		WriteListIfValid(streamWriter, availabilityTelemetry.Measurements, ",\"measurements\":{", "}");

		streamWriter.Write(",\"message\":\"");

		streamWriter.Write(availabilityTelemetry.Message);

		streamWriter.Write("\",\"name\":\"");

		streamWriter.Write(availabilityTelemetry.Name);

		streamWriter.Write("\"");

		WriteValueIfValid(streamWriter, availabilityTelemetry.RunLocation, ",\"runLocation\":\"", "\"");

		streamWriter.Write(",\"success\":");

		streamWriter.Write(success);
	}

	private static void WriteDataDependency(StreamWriter streamWriter, Telemetry telemetry)
	{
		var dependencyTelemetry = (DependencyTelemetry) telemetry;

		var success = dependencyTelemetry.Success ? "true" : "false";

		WriteValueIfValid(streamWriter, dependencyTelemetry.Data, "\"data\":\"", "\",");

		streamWriter.Write("\"duration\":\"");

		streamWriter.Write(dependencyTelemetry.Duration);

		streamWriter.Write("\",\"id\":\"");

		streamWriter.Write(dependencyTelemetry.Id);

		streamWriter.Write("\"");

		WriteListIfValid(streamWriter, dependencyTelemetry.Measurements, ",\"measurements\":{", "}");

		streamWriter.Write(",\"name\":\"");

		streamWriter.Write(dependencyTelemetry.Name);

		streamWriter.Write("\",\"success\":");

		streamWriter.Write(success);

		WriteValueIfValid(streamWriter, dependencyTelemetry.ResultCode, ",\"resultCode\":\"", "\"");

		WriteValueIfValid(streamWriter, dependencyTelemetry.Target, ",\"target\":\"", "\"");

		WriteValueIfValid(streamWriter, dependencyTelemetry.Type, ",\"type\":\"", "\"");
	}

	private static void WriteDataEvent(StreamWriter streamWriter, Telemetry telemetry)
	{
		var eventTelemetry = (EventTelemetry) telemetry;

		WriteListIfValid(streamWriter, eventTelemetry.Measurements, "\"measurements\":{", "},");

		streamWriter.Write("\"name\":\"");

		streamWriter.Write(eventTelemetry.Name);

		streamWriter.Write("\"");
	}

	private static void WriteDataException(StreamWriter streamWriter, Telemetry telemetry)
	{
		var exceptionTelemetry = (ExceptionTelemetry) telemetry;

		var exception = exceptionTelemetry.Exception;

		var outerId = 0;

		streamWriter.Write("\"exceptions\":[");

		do
		{
			var stackTrace = new System.Diagnostics.StackTrace(exception, true);

			var hasFullStack = stackTrace.FrameCount < ExceptionMaxStackLength;

			var id = exception.GetHashCode();

			var message = exception.Message.Replace("\r\n", " ");

			var frames = stackTrace.GetFrames();

			if (outerId != 0)
			{
				streamWriter.Write(",");
			}

			streamWriter.Write("{\"hasFullStack\":");

			streamWriter.Write(hasFullStack ? "true" : "false");

			streamWriter.Write(",\"id\":");

			streamWriter.Write(id);

			streamWriter.Write(",\"message\":\"");

			streamWriter.Write(message);

			streamWriter.Write("\",\"outerId\":");

			streamWriter.Write(outerId);

			streamWriter.Write(",\"parsedStack\":[");

			var takeFramesCount = Math.Min(frames.Length, ExceptionMaxStackLength);

			for (var frameIndex = 0; frameIndex < frames.Length || frameIndex < takeFramesCount; frameIndex++)
			{
				if (frameIndex != 0)
				{
					streamWriter.Write(",");
				}

				var frame = frames[frameIndex];

				var methodInfo = frame.GetMethod();

				streamWriter.Write("{\"assembly\":\"");

				streamWriter.Write(methodInfo?.Module.Assembly.FullName);

				streamWriter.Write("\",\"level\":");

				streamWriter.Write(frameIndex);

				streamWriter.Write(",\"line\":");

				streamWriter.Write(frame.GetFileLineNumber());

				streamWriter.Write(",\"method\":\"");

				if (methodInfo?.DeclaringType != null)
				{
					streamWriter.Write(methodInfo.DeclaringType.FullName);

					streamWriter.Write('.');
				}

				streamWriter.Write(methodInfo?.Name);

				streamWriter.Write("\"}");
			}

			streamWriter.Write("],\"typeName\":\"");

			streamWriter.Write(exception.GetType().FullName);

			streamWriter.Write("\"}");

			outerId = id;

			exception = exception.InnerException;
		}
		while (exception != null);

		streamWriter.Write("]");

		WriteListIfValid(streamWriter, exceptionTelemetry.Measurements, ",\"measurements\":{", "}");

		if (exceptionTelemetry.SeverityLevel.HasValue)
		{
			var severityLevelAsString = severityLevelToString[(Int32)exceptionTelemetry.SeverityLevel];

			WriteValue(streamWriter, severityLevelAsString, ",\"severityLevel\":\"", "\"");
		}
	}

	private static void WriteDataMetric(StreamWriter streamWriter, Telemetry telemetry)
	{
		var metricTelemetry = (MetricTelemetry) telemetry;

		streamWriter.Write("\"metrics\":[{");

		if (metricTelemetry.ValueAggregation != null)
		{
			streamWriter.Write("\"count\":");

			streamWriter.Write(metricTelemetry.ValueAggregation.Count);

			streamWriter.Write(",\"max\":");

			streamWriter.Write(metricTelemetry.ValueAggregation.Max);

			streamWriter.Write(",\"min\":");

			streamWriter.Write(metricTelemetry.ValueAggregation.Min);

			streamWriter.Write(",");
		}

		streamWriter.Write("\"name\":\"");

		streamWriter.Write(metricTelemetry.Name);

		streamWriter.Write("\",\"ns\":\"");

		streamWriter.Write(metricTelemetry.Namespace);

		streamWriter.Write("\",\"value\":");

		streamWriter.Write(metricTelemetry.Value);

		streamWriter.Write("}]");

		if (metricTelemetry.Properties != null && metricTelemetry.Properties.Count != 0)
		{
			streamWriter.Write(",\"properties\":{");

			_ = WriteList(streamWriter, metricTelemetry.Properties, false);

			streamWriter.Write("}");
		}
	}

	private static void WriteDataPageView(StreamWriter streamWriter, Telemetry telemetry)
	{
		var pageViewTelemetry = (PageViewTelemetry) telemetry;

		streamWriter.Write("\"duration\":\"");

		streamWriter.Write(pageViewTelemetry.Duration);

		streamWriter.Write("\",\"id\":\"");

		streamWriter.Write(pageViewTelemetry.Id);

		streamWriter.Write("\"");

		WriteListIfValid(streamWriter, pageViewTelemetry.Measurements, ",\"measurements\":{", "}");

		streamWriter.Write(",\"name\":\"");

		streamWriter.Write(pageViewTelemetry.Name);

		streamWriter.Write("\"");

		WriteValueIfValid(streamWriter, pageViewTelemetry.Url, ",\"url\":\"", "\"");
	}

	private static void WriteDataRequest(StreamWriter streamWriter, Telemetry telemetry)
	{
		var requestTelemetry = (RequestTelemetry) telemetry;

		streamWriter.Write("\"duration\":\"");

		streamWriter.Write(requestTelemetry.Duration);

		streamWriter.Write("\",\"id\":\"");

		streamWriter.Write(requestTelemetry.Id);

		streamWriter.Write("\"");

		WriteListIfValid(streamWriter, requestTelemetry.Measurements, ",\"measurements\":{", "}");

		streamWriter.Write(",\"name\":\"");

		streamWriter.Write(requestTelemetry.Name);

		streamWriter.Write("\",\"responseCode\":\"");

		streamWriter.Write(requestTelemetry.ResponseCode);

		streamWriter.Write("\",\"success\":");

		streamWriter.Write(requestTelemetry.Success ? "true" : "false");

		WriteValue(streamWriter, requestTelemetry.Url.ToString(), ",\"url\":\"", "\"");
	}

	private static void WriteDataTrace(StreamWriter streamWriter, Telemetry telemetry)
	{
		var traceTelemetry = (TraceTelemetry) telemetry;

		var severityLevelAsString = severityLevelToString[(Int32)traceTelemetry.SeverityLevel];

		streamWriter.Write("\"message\":\"");

		streamWriter.Write(traceTelemetry.Message);

		streamWriter.Write("\",\"severityLevel\":\"");

		streamWriter.Write(severityLevelAsString);

		streamWriter.Write("\"");
	}

	#endregion

	#region Methods: Helpers

	/// <summary>Serializes data as tags to the <paramref name="writer"/>.</summary>
	/// <param name="writer">The <see cref="StreamWriter"/> to write the serialized tags to.</param>
	/// <param name="telemetryOperation">The telemetry operation containing operation-specific tags.</param>
	/// <param name="telemetryTags">A read-only list of telemetry-specific key-value pair tags to serialize. Can be null.</param>
	/// <param name="trackerTags">A list of tags to attach to each telemetry item. From <see cref="TelemetryTracker"/>.</param>
	/// <param name="publisherTags">A list of tags to attach to each telemetry item. From <see cref="HttpTelemetryPublisher"/>.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteTags
	(
		StreamWriter writer,
		TelemetryOperation telemetryOperation,
		IReadOnlyList<KeyValuePair<String, String>>? telemetryTags,
		IReadOnlyList<KeyValuePair<String, String>>? trackerTags,
		IReadOnlyList<KeyValuePair<String, String>>? publisherTags
	)
	{
		var scopeHasItems = false;

		// serialize operation-specific tags
		scopeHasItems |= WriteKeyValue(writer, TelemetryTagKey.OperationId, telemetryOperation.Id, scopeHasItems);

		scopeHasItems |= WriteKeyValue(writer, TelemetryTagKey.OperationName, telemetryOperation.Name, scopeHasItems);

		scopeHasItems |= WriteKeyValue(writer, TelemetryTagKey.OperationParentId, telemetryOperation.ParentId, scopeHasItems);

		scopeHasItems |= WriteKeyValue(writer, TelemetryTagKey.OperationSyntheticSource, telemetryOperation.SyntheticSource, scopeHasItems);

		// serialize telemetry item tags
		scopeHasItems |= WriteList(writer, telemetryTags, scopeHasItems);

		// serialize tracker tags
		scopeHasItems |= WriteList(writer, trackerTags, scopeHasItems);

		// serialize common tags first
		_ = WriteList(writer, publisherTags, scopeHasItems);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteListIfValid
	(
		StreamWriter streamWriter,
		KeyValuePair<String, Double>[]? list,
		String pre,
		String post
	)
	{
		// serialize measurements
		if (list == null || list.Length == 0)
		{
			return;
		}

		streamWriter.Write(pre);

		WriteList(streamWriter, list);

		streamWriter.Write(post);
	}

	/// <summary>Serializes a list of key-value pairs into a JSON format and writes it to the <paramref name="streamWriter"/>.</summary>
	/// <param name="streamWriter">The StreamWriter to write the serialized JSON to.</param>
	/// <param name="list">The list of key-value pairs to serialize.</param>
	/// <param name="scopeHasItems">Indicates whether there are items already within the scope.</param>
	/// <returns>Returns <c>true</c> if at least one pair has been serialized within the scope, <c>false</c> otherwise.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Boolean WriteList
	(
		StreamWriter streamWriter,
		IReadOnlyList<KeyValuePair<String, String>>? list,
		Boolean scopeHasItems
	)
	{
		if (list == null || list.Count == 0)
		{
			return false;
		}

		for (var index = 0; index < list.Count; index++)
		{
			var pair = list[index];

			scopeHasItems |= WriteKeyValue(streamWriter, pair.Key, pair.Value, scopeHasItems);
		}

		return scopeHasItems;
	}

	/// <summary>Serializes a list of key-value pairs into a JSON format and writes it to the <paramref name="streamWriter"/>.</summary>
	/// <param name="streamWriter">The StreamWriter to write the serialized JSON to.</param>
	/// <param name="list">The list of key-value pairs to serialize.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteList
	(
		StreamWriter streamWriter,
		KeyValuePair<String, Double>[] list
	)
	{
		for (var index = 0; index < list.Length; index++)
		{
			var pair = list[index];

			var scopeHasItems = index != 0;

			WriteKeyValue(streamWriter, pair.Key, pair.Value, scopeHasItems);
		}
	}

	/// <summary>Serializes a key-value pair into a JSON format and writes it to the <paramref name="streamWriter"/>.</summary>
	/// <param name="streamWriter">The StreamWriter to write the serialized JSON to.</param>
	/// <param name="key">The key.</param>
	/// <param name="value">The value.</param>
	/// <param name="scopeHasItems">Indicates whether there are items already within the scope.</param>
	/// <returns>Returns <c>true</c> if at least one pair has been serialized within the scope, <c>false</c> otherwise.</returns>	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Boolean WriteKeyValue
	(
		StreamWriter streamWriter,
		String key,
		String? value,
		Boolean scopeHasItems
	)
	{
		if (String.IsNullOrEmpty(value))
		{
			return false;
		}

		if (scopeHasItems)
		{
			streamWriter.Write(",");
		}

		streamWriter.Write("\"");

		streamWriter.Write(key);

		streamWriter.Write("\":\"");

		streamWriter.Write(value);

		streamWriter.Write("\"");

		return true;
	}

	/// <summary>Serializes a key-value pair into a JSON format and writes it to the <paramref name="streamWriter"/>.</summary>
	/// <param name="streamWriter">The StreamWriter to write the serialized JSON to.</param>
	/// <param name="key">The key.</param>
	/// <param name="value">The value.</param>
	/// <param name="scopeHasItems">Indicates whether there are items already within the scope.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteKeyValue
	(
		StreamWriter streamWriter,
		String key,
		Double value,
		Boolean scopeHasItems
	)
	{
		if (scopeHasItems)
		{
			streamWriter.Write(',');
		}

		streamWriter.Write("\"");

		streamWriter.Write(key);

		streamWriter.Write("\":");

		streamWriter.Write(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteValueIfValid
	(
		StreamWriter streamWriter,
		String? value,
		String pre,
		String post
	)
	{
		if (String.IsNullOrWhiteSpace(value))
		{
			return;
		}

		WriteValue(streamWriter, value, pre, post);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteValueIfValid
	(
		StreamWriter streamWriter,
		Uri? value,
		String pre,
		String post
	)
	{
		if (value == null)
		{
			return;
		}

		WriteValue(streamWriter, value.ToString(), pre, post);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteValue
	(
		StreamWriter streamWriter,
		String? value,
		String pre,
		String post
	)
	{
		streamWriter.Write(pre);

		streamWriter.Write(value);

		streamWriter.Write(post);
	}

	#endregion
}
