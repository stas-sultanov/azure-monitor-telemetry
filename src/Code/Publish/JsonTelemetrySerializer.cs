// Authored by Stas Sultanov
// Copyright © Stas Sultanov

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

	private const String Name_Availability = "AppAvailabilityResults";
	private const String Name_Dependency = "AppDependencies";
	private const String Name_Event = "AppEvents";
	private const String Name_Exception = "AppExceptions";
	private const String Name_Metric = "AppMetrics";
	private const String Name_PageView = "AppPageViews";
	private const String Name_Request = "AppRequests";
	private const String Name_Trace = "AppTraces";
	private const String Type_Availability = "AvailabilityData";
	private const String Type_Dependency = "RemoteDependencyData";
	private const String Type_Event = "EventData";
	private const String Type_Exception = "ExceptionData";
	private const String Type_Metric = "MetricData";
	private const String Type_PageView = "PageViewData";
	private const String Type_Request = "RequestData";
	private const String Type_Trace = "MessageData";

	#endregion

	#region Static Fields

	/// <summary>
	/// <see cref="SeverityLevel"/> to <see cref="String"/> mapping.
	/// </summary>
	private static readonly String[] severityLevelToString = ["Verbose", "Information", "Warning", "Error", "Critical"];

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

		if (!propertiesOnTop && telemetry.Properties is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "properties", telemetry.Properties);
		}

		streamWriter.Write("}");

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "baseType", baseType);

		streamWriter.Write("}");

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "iKey", instrumentationKey);

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "name", name);

		// serialize properties
		if (propertiesOnTop && telemetry.Properties is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "properties", telemetry.Properties);
		}

		if (telemetry.Tags is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "tags", telemetry.Tags);
		}

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "time", telemetry.Time);

		streamWriter.Write("}");
	}

	#endregion

	#region Methods: Write Telemetry Data

	private static void WriteDataAvailability(StreamWriter streamWriter, Telemetry telemetry)
	{
		var availabilityTelemetry = (AvailabilityTelemetry) telemetry;

		WriteProperty(streamWriter, "duration", availabilityTelemetry.Duration);

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "id", availabilityTelemetry.Id);

		if (availabilityTelemetry.Measurements is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "measurements", availabilityTelemetry.Measurements);
		}

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "message", availabilityTelemetry.Message);

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "name", availabilityTelemetry.Name);

		if (availabilityTelemetry.RunLocation is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "runLocation", availabilityTelemetry.RunLocation);
		}

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "success", availabilityTelemetry.Success);
	}

	private static void WriteDataDependency(StreamWriter streamWriter, Telemetry telemetry)
	{
		var dependencyTelemetry = (DependencyTelemetry) telemetry;

		if (dependencyTelemetry.Data is not null)
		{
			WriteProperty(streamWriter, "data", dependencyTelemetry.Data!);

			WriteComa(streamWriter);
		}

		WriteProperty(streamWriter, "duration", dependencyTelemetry.Duration);

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "id", dependencyTelemetry.Id);

		if (dependencyTelemetry.Measurements is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "measurements", dependencyTelemetry.Measurements);
		}

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "name", dependencyTelemetry.Name);

		if (dependencyTelemetry.ResultCode is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "resultCode", dependencyTelemetry.ResultCode);
		}

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "success", dependencyTelemetry.Success);

		if (dependencyTelemetry.Target is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "target", dependencyTelemetry.Target);
		}

		if (dependencyTelemetry.Type is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "type", dependencyTelemetry.Type);
		}
	}

	private static void WriteDataEvent(StreamWriter streamWriter, Telemetry telemetry)
	{
		var eventTelemetry = (EventTelemetry) telemetry;

		if (eventTelemetry.Measurements is not null)
		{
			WriteProperty(streamWriter, "measurements", eventTelemetry.Measurements);

			WriteComa(streamWriter);
		}

		WriteProperty(streamWriter, "name", eventTelemetry.Name);
	}

	private static void WriteDataException(StreamWriter streamWriter, Telemetry telemetry)
	{
		var exceptionTelemetry = (ExceptionTelemetry) telemetry;

		streamWriter.Write("\"exceptions\":[");

		for (var exceptionInfoIndex = 0; exceptionInfoIndex < exceptionTelemetry.Exceptions.Count; exceptionInfoIndex++)
		{
			// get exception info
			var exceptionInfo = exceptionTelemetry.Exceptions[exceptionInfoIndex];

			if (exceptionInfoIndex != 0)
			{
				WriteComa(streamWriter);
			}

			streamWriter.Write("{");

			WriteProperty(streamWriter, "hasFullStack", exceptionInfo.HasFullStack);

			WriteComa(streamWriter);
			WriteProperty(streamWriter, "id", exceptionInfo.Id);

			WriteComa(streamWriter);
			WriteProperty(streamWriter, "message", exceptionInfo.Message);

			WriteComa(streamWriter);
			WriteProperty(streamWriter, "outerId", exceptionInfo.OuterId);

			if (exceptionInfo.ParsedStack is not null)
			{
				streamWriter.Write(",\"parsedStack\":[");

				for (var frameIndex = 0; frameIndex < exceptionInfo.ParsedStack.Count; frameIndex++)
				{
					var frame = exceptionInfo.ParsedStack[frameIndex];

					if (frameIndex != 0)
					{
						streamWriter.Write(",");
					}

					streamWriter.Write("{");

					WriteProperty(streamWriter, "assembly", frame.Assembly);

					if (frame.FileName is not null)
					{
						WriteComa(streamWriter);
						WriteProperty(streamWriter, "fileName", frame.FileName);
					}

					WriteComa(streamWriter);
					WriteProperty(streamWriter, "level", frame.Level);

					WriteComa(streamWriter);
					WriteProperty(streamWriter, "line", frame.Line);

					if (frame.Method is not null)
					{
						WriteComa(streamWriter);
						WriteProperty(streamWriter, "method", frame.Method);
					}

					streamWriter.Write("}");
				}

				streamWriter.Write("]");
			}

			WriteComa(streamWriter);
			WriteProperty(streamWriter, "typeName", exceptionInfo.TypeName);

			streamWriter.Write("}");
		}

		streamWriter.Write("]");

		if (exceptionTelemetry.Measurements is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "measurements", exceptionTelemetry.Measurements);
		}

		if (exceptionTelemetry.ProblemId is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "problemId", exceptionTelemetry.ProblemId);
		}

		if (exceptionTelemetry.SeverityLevel.HasValue)
		{
			var severityLevelAsString = severityLevelToString[(Int32)exceptionTelemetry.SeverityLevel.Value];

			WriteComa(streamWriter);
			WriteProperty(streamWriter, "severityLevel", severityLevelAsString);
		}
	}

	private static void WriteDataMetric(StreamWriter streamWriter, Telemetry telemetry)
	{
		var metricTelemetry = (MetricTelemetry) telemetry;

		streamWriter.Write("\"metrics\":[{");

		if (metricTelemetry.ValueAggregation is not null)
		{
			WriteProperty(streamWriter, "count", metricTelemetry.ValueAggregation.Count);

			WriteComa(streamWriter);
			WriteProperty(streamWriter, "max", metricTelemetry.ValueAggregation.Max);

			WriteComa(streamWriter);
			WriteProperty(streamWriter, "min", metricTelemetry.ValueAggregation.Min);

			WriteComa(streamWriter);
		}

		WriteProperty(streamWriter, "name", metricTelemetry.Name);

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "ns", metricTelemetry.Namespace);

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "value", metricTelemetry.Value);

		streamWriter.Write("}]");
	}

	private static void WriteDataPageView(StreamWriter streamWriter, Telemetry telemetry)
	{
		var pageViewTelemetry = (PageViewTelemetry) telemetry;

		WriteProperty(streamWriter, "duration", pageViewTelemetry.Duration);

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "id", pageViewTelemetry.Id);

		if (pageViewTelemetry.Measurements is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "measurements", pageViewTelemetry.Measurements);
		}

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "name", pageViewTelemetry.Name);

		if (pageViewTelemetry.Url is not null)
		{
			var urlAsString = pageViewTelemetry.Url.ToString();

			WriteComa(streamWriter);
			WriteProperty(streamWriter, "url", urlAsString!);
		}
	}

	private static void WriteDataRequest(StreamWriter streamWriter, Telemetry telemetry)
	{
		var requestTelemetry = (RequestTelemetry) telemetry;

		var urlAsString = requestTelemetry.Url.ToString();

		WriteProperty(streamWriter, "id", requestTelemetry.Id);

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "duration", requestTelemetry.Duration);

		if (requestTelemetry.Measurements is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "measurements", requestTelemetry.Measurements);
		}

		if (requestTelemetry.Name is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "name", requestTelemetry.Name!);
		}

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "responseCode", requestTelemetry.ResponseCode);

		if (requestTelemetry.Name is not null)
		{
			WriteComa(streamWriter);
			WriteProperty(streamWriter, "source", requestTelemetry.Source!);
		}

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "success", requestTelemetry.Success);

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "url", urlAsString);
	}

	private static void WriteDataTrace(StreamWriter streamWriter, Telemetry telemetry)
	{
		var traceTelemetry = (TraceTelemetry) telemetry;

		var severityLevelAsString = severityLevelToString[(Int32)traceTelemetry.SeverityLevel];

		WriteProperty(streamWriter, "message", traceTelemetry.Message);

		WriteComa(streamWriter);
		WriteProperty(streamWriter, "severityLevel", severityLevelAsString);
	}

	#endregion

	#region Methods: Write Helpers

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteComa
	(
		StreamWriter streamWriter
	)
	{
		streamWriter.Write(",");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteProperty
	(
		StreamWriter streamWriter,
		String name,
		String value
	)
	{
		streamWriter.Write("\"");

		streamWriter.Write(name);

		streamWriter.Write("\":\"");

		streamWriter.Write(value);

		streamWriter.Write("\"");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteProperty
	(
		StreamWriter streamWriter,
		String name,
		Boolean value
	)
	{
		streamWriter.Write("\"");

		var valueAsString = value ? "true" : "false";

		streamWriter.Write(name);

		streamWriter.Write("\":");

		streamWriter.Write(valueAsString);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteProperty
	(
		StreamWriter streamWriter,
		String name,
		Double value
	)
	{
		streamWriter.Write("\"");

		streamWriter.Write(name);

		streamWriter.Write("\":");

		streamWriter.Write(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteProperty
	(
		StreamWriter streamWriter,
		String name,
		Int32 value
	)
	{
		streamWriter.Write("\"");

		streamWriter.Write(name);

		streamWriter.Write("\":");

		streamWriter.Write(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteProperty
	(
		StreamWriter streamWriter,
		String name,
		DateTime value
	)
	{
		var valueAsString = value.ToString("O");

		WriteProperty(streamWriter, name, valueAsString);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteProperty
	(
		StreamWriter streamWriter,
		String name,
		TimeSpan value
	)
	{
		var valueAsString = value.ToString(null, streamWriter.FormatProvider);

		WriteProperty(streamWriter, name, valueAsString);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteProperty
	(
		StreamWriter streamWriter,
		String name,
		IReadOnlyList<KeyValuePair<String, Double>> value
	)
	{
		streamWriter.Write("\"");

		streamWriter.Write(name);

		streamWriter.Write("\":{");

		for (var index = 0; index < value.Count; index++)
		{
			var pair = value[index];

			if (index != 0)
			{
				WriteComa(streamWriter);
			}

			WriteProperty(streamWriter, pair.Key, pair.Value);
		}

		streamWriter.Write("}");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteProperty
	(
		StreamWriter streamWriter,
		String name,
		IReadOnlyList<KeyValuePair<String, String>> value
	)
	{
		streamWriter.Write("\"");

		streamWriter.Write(name);

		streamWriter.Write("\":{");

		for (var index = 0; index < value.Count; index++)
		{
			var pair = value[index];

			if (index != 0)
			{
				WriteComa(streamWriter);
			}

			WriteProperty(streamWriter, pair.Key, pair.Value);
		}

		streamWriter.Write("}");
	}

	#endregion
}
