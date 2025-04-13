// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Tests;

using System;
using System.Diagnostics;

using Azure.Monitor.Telemetry.Models;

/// <summary>
/// Provides a set of method to create types that implement <see cref="Telemetry"/> for test purposes.
/// </summary>
internal sealed class TelemetryFactory
{
	#region Static Methods: Helpers

	internal static String GetOperationId()
	{
		return ActivityTraceId.CreateRandom().ToString();
	}

	internal static String GetActivityId()
	{
		return ActivitySpanId.CreateRandom().ToString();
	}

	internal static TimeSpan GetRandomDuration
	(
		Int32 millisecondsMin,
		Int32 millisecondsMax
	)
	{
		var milliseconds = Random.Shared.Next(millisecondsMin, millisecondsMax);

		return TimeSpan.FromMilliseconds(milliseconds);
	}

	internal static void Simulate_ExceptionThrow
	(
		String? param1
	)
	{
		throw new ArgumentNullException(nameof(param1), "L1");
	}

	internal static void Simulate_ExceptionThrow_WithInnerException
	(
		String? paramL2
	)
	{
		try
		{
			Simulate_ExceptionThrow(paramL2);
		}
		catch (Exception exception)
		{
			throw new Exception("L2", exception);
		}
	}

	#endregion

	#region Static Methods: Create

	/// <summary>
	/// Creates instance of <see cref="AvailabilityTelemetry"/> with minimum load.
	/// </summary>
	public static AvailabilityTelemetry Create_AvailabilityTelemetry_Min
	(
		String name,
		String message = @"Passed"
	)
	{
		var id = GetActivityId();

		var result = new AvailabilityTelemetry
		{
			Duration = TimeSpan.Zero,
			Id = id,
			Message = message,
			Name = name,
			Success = true,
			Time = DateTime.UtcNow
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="DependencyTelemetry"/> with minimum load.
	/// </summary>
	public static DependencyTelemetry Create_DependencyTelemetry_Min
	(
		String name
	)
	{
		var id = GetActivityId();

		var result = new DependencyTelemetry
		{
			Time = DateTime.UtcNow,
			Id = id,
			Name = name
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="EventTelemetry"/> with minimum load.
	/// </summary>
	public static EventTelemetry Create_EventTelemetry_Min
	(
		String name
	)
	{
		var result = new EventTelemetry
		{
			Name = name,
			Time = DateTime.UtcNow,
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="ExceptionTelemetry"/> with minimum load.
	/// </summary>
	public static ExceptionTelemetry Create_ExceptionTelemetry_Min()
	{
		try
		{
			Simulate_ExceptionThrow(null);

			throw new Exception();
		}
		catch (Exception exception)
		{
			var exceptions = TelemetryUtils.ConvertExceptionToModel(exception);

			var result = new ExceptionTelemetry
			{
				Exceptions = exceptions,
				Time = DateTime.UtcNow
			};

			return result;
		}
	}

	/// <summary>
	/// Creates instance of <see cref="MetricTelemetry"/> with minimum load.
	/// </summary>
	public static MetricTelemetry Create_MetricTelemetry_Min
	(
		String @namespace,
		String name,
		Double value
	)
	{
		var result = new MetricTelemetry
		{
			Name = name,
			Namespace = @namespace,
			Time = DateTime.UtcNow,
			Value = value
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="PageViewTelemetry"/> with minimum load.
	/// </summary>
	public static PageViewTelemetry Create_PageViewTelemetry_Min
	(
		String name
	)
	{
		var id = GetActivityId();

		var result = new PageViewTelemetry
		{
			Id = id,
			Name = name,
			Time = DateTime.UtcNow
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="TraceTelemetry"/> with minimum load.
	/// </summary>
	public static TraceTelemetry Create_TraceTelemetry_Min(String message)
	{
		var result = new TraceTelemetry
		{
			Message = message,
			SeverityLevel = SeverityLevel.Verbose,
			Time = DateTime.UtcNow
		};

		return result;
	}

	#endregion

	#region Properties

	public KeyValuePair<String, Double>[] Measurements { get; init; } = [];
	public KeyValuePair<String, String>[] Properties { get; init; } = [];
	public KeyValuePair<String, String>[] Tags { get; init; } = [];

	#endregion

	#region Methods: Create

	/// <summary>
	/// Creates instance of <see cref="AvailabilityTelemetry"/> with full load.
	/// </summary>
	public AvailabilityTelemetry Create_AvailabilityTelemetry_Max
	(
		String name,
		String message = @"Passed"
	)
	{
		var id = GetActivityId();

		var duration = GetRandomDuration(100, 2000);

		var result = new AvailabilityTelemetry
		{
			Duration = duration,
			Id = id,
			Measurements = Measurements,
			Message = message,
			Name = name,
			Properties = Properties,
			RunLocation = "Earth",
			Success = true,
			Tags = Tags,
			Time = DateTime.UtcNow
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="DependencyTelemetry"/> with full load.
	/// </summary>
	public DependencyTelemetry Create_DependencyTelemetry_Max
	(
		String name,
		Uri url
	)
	{
		var id = GetActivityId();

		var type = TelemetryUtils.DetectDependencyTypeFromHttpUri(url);

		var duration = GetRandomDuration(200, 800);

		var result = new DependencyTelemetry
		{
			Data = "data",
			Duration = duration,
			Id = id,
			Measurements = Measurements,
			Name = name,
			Properties = Properties,
			ResultCode = "401",
			Success = false,
			Tags = Tags,
			Target = "target",
			Time = DateTime.UtcNow,
			Type = type
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="EventTelemetry"/> with full load.
	/// </summary>
	public EventTelemetry Create_EventTelemetry_Max
	(
		String name
	)
	{
		var result = new EventTelemetry
		{
			Measurements = Measurements,
			Name = name,
			Properties = Properties,
			Tags = Tags,
			Time = DateTime.UtcNow
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="ExceptionTelemetry"/> with full load.
	/// </summary>
	public ExceptionTelemetry Create_ExceptionTelemetry_Max
	(
		String? problemId = null,
		SeverityLevel? severityLevel = null
	)
	{
		try
		{
			Simulate_ExceptionThrow_WithInnerException(null);

			throw new Exception();
		}
		catch (Exception exception)
		{
			var exceptions = TelemetryUtils.ConvertExceptionToModel(exception);

			var result = new ExceptionTelemetry
			{
				Exceptions = exceptions,
				Measurements = Measurements,
				Properties = Properties,
				ProblemId = problemId,
				SeverityLevel = severityLevel,
				Tags = Tags,
				Time = DateTime.UtcNow
			};

			return result;
		}
	}

	/// <summary>
	/// Creates instance of <see cref="MetricTelemetry"/> with full load.
	/// </summary>
	public MetricTelemetry Create_MetricTelemetry_Max
	(
		String @namespace,
		String name,
		Double value,
		MetricValueAggregation aggregation
	)
	{
		var result = new MetricTelemetry
		{
			Time = DateTime.UtcNow,
			Namespace = @namespace,
			Name = name,
			Value = value,
			ValueAggregation = aggregation,
			Properties = Properties,
			Tags = Tags
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="PageViewTelemetry"/> with full load.
	/// </summary>
	public PageViewTelemetry Create_PageViewTelemetry_Max
	(
		String name,
		Uri url
	)
	{
		var id = GetActivityId();

		var duration = GetRandomDuration(300, 700);

		var result = new PageViewTelemetry
		{
			Measurements = Measurements,
			Duration = duration,
			Id = id,
			Name = name,
			Properties = Properties,
			Tags = Tags,
			Time = DateTime.UtcNow,
			Url = url
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="RequestTelemetry"/> with full load.
	/// </summary>
	public RequestTelemetry Create_RequestTelemetry_Max
	(
		String name,
		Uri url
	)
	{
		var id = GetActivityId();

		var result = new RequestTelemetry
		{
			Duration = TimeSpan.FromSeconds(1),
			Id = id,
			Measurements = Measurements,
			Name = name,
			Properties = Properties,
			ResponseCode = "200",
			Success = true,
			Tags = Tags,
			Time = DateTime.UtcNow,
			Url = url
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="TraceTelemetry"/> with full load.
	/// </summary>
	public TraceTelemetry Create_TraceTelemetry_Max
	(
		String message
	)
	{
		var result = new TraceTelemetry
		{
			Message = message,
			Properties = Properties,
			SeverityLevel = SeverityLevel.Information,
			Tags = Tags,
			Time = DateTime.UtcNow
		};

		return result;
	}

	#endregion
}
