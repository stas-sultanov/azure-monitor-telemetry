// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Publish;

using System;
using System.IO;
using System.Runtime.CompilerServices;

using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Models;

/// <summary>
/// Provides serialization of types that implements <see cref="Telemetry"/> into the stream using JSON format.
/// </summary>
/// <remarks>Uses version 2 of the HTTP API of the Azure Monitor service.</remarks>
public static class JsonTelemetrySerializer
{
	#region Constants

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
	public static void Serialize
	(
		StreamWriter streamWriter,
		String instrumentationKey,
		Telemetry telemetry
	)
	{
		String name;

		String baseType;

		Action<StreamWriter, Telemetry> writeData;

		// MS Engineers are not familiar with the term "CONSISTENCY" and it's meaning.
		// for Availability and TelemetryMetric, Properties are in the other place of the data structure...
		Boolean propertiesOnTop;

		switch (telemetry)
		{
			case AvailabilityTelemetry:
				name = Name_Availability;
				baseType = Type_Availability;
				writeData = WriteDataAvailability;
				propertiesOnTop = false;
				break;
			case DependencyTelemetry:
				name = Name_Dependency;
				baseType = Type_Dependency;
				writeData = WriteDataDependency;
				propertiesOnTop = true;
				break;
			case EventTelemetry:
				name = Name_Event;
				baseType = Type_Event;
				writeData = WriteDataEvent;
				propertiesOnTop = true;
				break;
			case ExceptionTelemetry:
				name = Name_Exception;
				baseType = Type_Exception;
				writeData = WriteDataException;
				propertiesOnTop = true;
				break;
			case MetricTelemetry:
				name = Name_Metric;
				baseType = Type_Metric;
				writeData = WriteDataMetric;
				propertiesOnTop = false;
				break;
			case PageViewTelemetry:
				name = Name_PageView;
				baseType = Type_PageView;
				writeData = WriteDataPageView;
				propertiesOnTop = true;
				break;
			case RequestTelemetry:
				name = Name_Request;
				baseType = Type_Request;
				writeData = WriteDataRequest;
				propertiesOnTop = true;
				break;
			case TraceTelemetry:
				name = Name_Trace;
				baseType = Type_Trace;
				writeData = WriteDataTrace;
				propertiesOnTop = true;
				break;
			default:
				return;
		}

		streamWriter.Write("{\"data\":{\"baseData\":{");

		writeData(streamWriter, telemetry);

		if (!propertiesOnTop)
		{
			WriteOptional(streamWriter, ",\"properties\":{", "}", telemetry.Properties);
		}

		streamWriter.Write("},\"baseType\":\"");

		streamWriter.Write(baseType);

		streamWriter.Write("\"},\"iKey\":\"");

		streamWriter.Write(instrumentationKey);

		streamWriter.Write("\",\"name\":\"");

		streamWriter.Write(name);

		streamWriter.Write("\"");

		// serialize properties
		if (propertiesOnTop)
		{
			WriteOptional(streamWriter, ",\"properties\":{", "}", telemetry.Properties);
		}

		WriteOptional(streamWriter, ",\"tags\":{", "}", telemetry.Tags);

		streamWriter.Write(",\"time\":\"");

		streamWriter.Write(telemetry.Time.ToString("O"));

		streamWriter.Write("\"}");
	}

	#endregion

	#region Methods: Write Telemetry Data

	private static void WriteDataAvailability(StreamWriter streamWriter, Telemetry telemetry)
	{
		var availabilityTelemetry = (AvailabilityTelemetry) telemetry;

		var success = availabilityTelemetry.Success ? "true" : "false";

		streamWriter.Write("\"duration\":\"");

		streamWriter.Write(availabilityTelemetry.Duration);

		streamWriter.Write("\",\"id\":\"");

		streamWriter.Write(availabilityTelemetry.Id);

		streamWriter.Write("\"");

		WriteOptional(streamWriter, ",\"measurements\":{", "}", availabilityTelemetry.Measurements);

		streamWriter.Write(",\"message\":\"");

		streamWriter.Write(availabilityTelemetry.Message);

		streamWriter.Write("\",\"name\":\"");

		streamWriter.Write(availabilityTelemetry.Name);

		streamWriter.Write("\"");

		WriteOptional(streamWriter, ",\"runLocation\":\"", "\"", availabilityTelemetry.RunLocation);

		streamWriter.Write(",\"success\":");

		streamWriter.Write(success);
	}

	private static void WriteDataDependency(StreamWriter streamWriter, Telemetry telemetry)
	{
		var dependencyTelemetry = (DependencyTelemetry) telemetry;

		var success = dependencyTelemetry.Success ? "true" : "false";

		WriteOptional(streamWriter, "\"data\":\"", "\",", dependencyTelemetry.Data);

		streamWriter.Write("\"duration\":\"");

		streamWriter.Write(dependencyTelemetry.Duration);

		streamWriter.Write("\",\"id\":\"");

		streamWriter.Write(dependencyTelemetry.Id);

		streamWriter.Write("\"");

		WriteOptional(streamWriter, ",\"measurements\":{", "}", dependencyTelemetry.Measurements);

		streamWriter.Write(",\"name\":\"");

		streamWriter.Write(dependencyTelemetry.Name);

		streamWriter.Write("\",\"success\":");

		streamWriter.Write(success);

		WriteOptional(streamWriter, ",\"resultCode\":\"", "\"", dependencyTelemetry.ResultCode);

		WriteOptional(streamWriter, ",\"target\":\"", "\"", dependencyTelemetry.Target);

		WriteOptional(streamWriter, ",\"type\":\"", "\"", dependencyTelemetry.Type);
	}

	private static void WriteDataEvent(StreamWriter streamWriter, Telemetry telemetry)
	{
		var eventTelemetry = (EventTelemetry) telemetry;

		WriteOptional(streamWriter, "\"measurements\":{", "},", eventTelemetry.Measurements);

		streamWriter.Write("\"name\":\"");

		streamWriter.Write(eventTelemetry.Name);

		streamWriter.Write("\"");
	}

	private static void WriteDataException(StreamWriter streamWriter, Telemetry telemetry)
	{
		var exceptionTelemetry = (ExceptionTelemetry) telemetry;

		streamWriter.Write("\"exceptions\":[");

		for (var exceptionInfoIndex = 0; exceptionInfoIndex < exceptionTelemetry.Exceptions.Count; exceptionInfoIndex++)
		{
			// get exception info
			var exceptionInfo = exceptionTelemetry.Exceptions[exceptionInfoIndex];

			if (exceptionInfoIndex > 0)
			{
				streamWriter.Write(",");
			}

			streamWriter.Write("{\"hasFullStack\":");

			streamWriter.Write(exceptionInfo.HasFullStack ? "true" : "false");

			streamWriter.Write(",\"id\":");

			streamWriter.Write(exceptionInfo.Id);

			streamWriter.Write(",\"message\":\"");

			streamWriter.Write(exceptionInfo.Message);

			streamWriter.Write("\",\"outerId\":");

			streamWriter.Write(exceptionInfo.OuterId);

			if (exceptionInfo.ParsedStack != null)
			{
				streamWriter.Write(",\"parsedStack\":[");

				for (var frameIndex = 0; frameIndex < exceptionInfo.ParsedStack.Count; frameIndex++)
				{
					if (frameIndex != 0)
					{
						streamWriter.Write(",");
					}

					var frame = exceptionInfo.ParsedStack[frameIndex];

					streamWriter.Write("{\"assembly\":\"");

					streamWriter.Write(frame.Assembly);

					WriteOptional(streamWriter, "\",\"fileName\":\"", "", frame.FileName);

					streamWriter.Write("\",\"level\":");

					streamWriter.Write(frame.Level);

					streamWriter.Write(",\"line\":");

					streamWriter.Write(frame.Line);

					streamWriter.Write(",\"method\":\"");

					streamWriter.Write(frame.Method);

					streamWriter.Write("\"}");
				}

				streamWriter.Write("]");
			}

			streamWriter.Write(",\"typeName\":\"");

			streamWriter.Write(exceptionInfo.TypeName);

			streamWriter.Write("\"}");
		}

		streamWriter.Write("]");

		WriteOptional(streamWriter, ",\"measurements\":{", "}", exceptionTelemetry.Measurements);

		WriteOptional(streamWriter, ",\"problemId\":\"", "\"", exceptionTelemetry.ProblemId);

		if (exceptionTelemetry.SeverityLevel.HasValue)
		{
			var severityLevelAsString = severityLevelToString[(Int32)exceptionTelemetry.SeverityLevel];

			Write(streamWriter, ",\"severityLevel\":\"", "\"", severityLevelAsString);
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
	}

	private static void WriteDataPageView(StreamWriter streamWriter, Telemetry telemetry)
	{
		var pageViewTelemetry = (PageViewTelemetry) telemetry;

		streamWriter.Write("\"duration\":\"");

		streamWriter.Write(pageViewTelemetry.Duration);

		streamWriter.Write("\",\"id\":\"");

		streamWriter.Write(pageViewTelemetry.Id);

		streamWriter.Write("\"");

		WriteOptional(streamWriter, ",\"measurements\":{", "}", pageViewTelemetry.Measurements);

		streamWriter.Write(",\"name\":\"");

		streamWriter.Write(pageViewTelemetry.Name);

		streamWriter.Write("\"");

		if (pageViewTelemetry.Url != null)
		{
			Write(streamWriter, ",\"url\":\"", "\"", pageViewTelemetry.Url.ToString());
		}
	}

	private static void WriteDataRequest(StreamWriter streamWriter, Telemetry telemetry)
	{
		var requestTelemetry = (RequestTelemetry) telemetry;

		Write(streamWriter, "\"duration\":\"", "\"", requestTelemetry.Duration);

		Write(streamWriter, ",\"id\":\"", "\"", requestTelemetry.Id);

		WriteOptional(streamWriter, ",\"measurements\":{", "}", requestTelemetry.Measurements);

		WriteOptional(streamWriter, ",\"name\":\"", "\"", requestTelemetry.Name);

		Write(streamWriter, ",\"responseCode\":\"", "\"", requestTelemetry.ResponseCode);

		WriteOptional(streamWriter, ",\"source\":\"", "\"", requestTelemetry.Source);

		streamWriter.Write(",\"success\":");

		streamWriter.Write(requestTelemetry.Success ? "true" : "false");

		Write(streamWriter, ",\"url\":\"", "\"", requestTelemetry.Url.ToString());
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

	#region Methods: Write Helpers

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Write
	(
		StreamWriter streamWriter,
		String pre,
		String post,
		Boolean value
	)
	{
		var valueAsString = value ? "true" : "false";

		streamWriter.Write(pre);

		streamWriter.Write(valueAsString);

		streamWriter.Write(post);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Write
	(
		StreamWriter streamWriter,
		String pre,
		String post,
		TimeSpan value
	)
	{
		streamWriter.Write(pre);

		streamWriter.Write(value);

		streamWriter.Write(post);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Write
	(
		StreamWriter streamWriter,
		String pre,
		String post,
		String value
	)
	{
		streamWriter.Write(pre);

		streamWriter.Write(value);

		streamWriter.Write(post);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteOptional
	(
		StreamWriter streamWriter,
		String pre,
		String post,
		String? value
	)
	{
		if (value == null)
		{
			return;
		}

		Write(streamWriter, pre, post, value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteOptional
	(
		StreamWriter streamWriter,
		String pre,
		String post,
		IReadOnlyList<KeyValuePair<String, Double>>? list
	)
	{
		if (list == null || list.Count == 0)
		{
			return;
		}

		streamWriter.Write(pre);

		var scopeHasItems = false;

		for (var index = 0; index < list.Count; index++)
		{
			var pair = list[index];

			if (String.IsNullOrWhiteSpace(pair.Key))
			{
				continue;
			}

			WritePair(streamWriter, pair.Key, pair.Value, scopeHasItems);

			scopeHasItems = true;
		}

		streamWriter.Write(post);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteOptional
	(
		StreamWriter streamWriter,
		String pre,
		String post,
		IReadOnlyList<KeyValuePair<String, String>>? list
	)
	{
		if (list == null || list.Count == 0)
		{
			return;
		}

		streamWriter.Write(pre);

		var scopeHasItems = false;

		for (var index = 0; index < list.Count; index++)
		{
			var pair = list[index];

			if (String.IsNullOrWhiteSpace(pair.Key) || String.IsNullOrWhiteSpace(pair.Value))
			{
				continue;
			}

			WritePair(streamWriter, pair.Key, pair.Value, scopeHasItems);

			scopeHasItems = true;
		}

		streamWriter.Write(post);
	}

	#endregion

	#region Methods: Write Pair

	/// <summary>Writes a key-value into the <paramref name="streamWriter"/>.</summary>
	/// <param name="streamWriter">The writer.</param>
	/// <param name="key">The key.</param>
	/// <param name="value">The value.</param>
	/// <param name="scopeHasItems">A flag that indicates if there are items already within the scope.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WritePair
	(
		StreamWriter streamWriter,
		String key,
		String value,
		Boolean scopeHasItems
	)
	{
		if (scopeHasItems)
		{
			streamWriter.Write(",");
		}

		streamWriter.Write("\"");

		streamWriter.Write(key);

		streamWriter.Write("\":\"");

		streamWriter.Write(value);

		streamWriter.Write("\"");
	}

	/// <summary>Writes a key-value into the <paramref name="streamWriter"/>.</summary>
	/// <param name="streamWriter">The writer.</param>
	/// <param name="key">The key.</param>
	/// <param name="value">The value.</param>
	/// <param name="scopeHasItems">A flag that indicates if there are items already within the scope.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WritePair
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

	#endregion
}
