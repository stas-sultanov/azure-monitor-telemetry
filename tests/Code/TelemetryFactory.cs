// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Tests;

using System;
using System.Diagnostics;

/// <summary>
/// Provides instances of classes that implements <see cref="Telemetry"/> for testing purposes.
/// </summary>
internal sealed class TelemetryFactory
{
	#region Properties

	public KeyValuePair<String, Double>[] Measurements { get; set; }
	public String Message { get; set; }
	public String Name { get; set; }
	public TelemetryOperation Operation { get; set; }
	public KeyValuePair<String, String>[] Properties { get; set; }
	public KeyValuePair<String, String>[] Tags { get; set; }
	public Uri Url { get; set; }

	#endregion

	#region Constructors

	internal TelemetryFactory()
	{
		Message = "message";

		Measurements = [new("m", 0), new("n", 1.5)];

		Name = "name";

		Operation = new TelemetryOperation
		{
			Id = Guid.NewGuid().ToString("N"),
			Name = "Test #" + DateTime.Now.ToString("yyMMddhhmm")
		};

		Properties = [new("key", "value")];

		Tags = [new(TelemetryTagKey.CloudRole, "TestMachine")];

		Url = new Uri("https://gostas.dev");
	}

	#endregion

	#region Methods: Helpers

	internal static String GetId()
	{
		return ActivitySpanId.CreateRandom().ToString();
	}

	internal static TimeSpan GetRandomDuration(Int32 millisecondsMin, Int32 millisecondsMax)
	{
		var milliseconds = Random.Shared.Next(millisecondsMin, millisecondsMax);

		return TimeSpan.FromMilliseconds(milliseconds);
	}

	internal static void Simulate_ExceptionThrow(String? param1)
	{
		throw new ArgumentNullException(nameof(param1), "L1");
	}

	internal static void Simulate_ExceptionThrow_WithInnerException(String? paramL2)
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

	#region Methods: Create

	/// <summary>
	/// Creates instance of <see cref="AvailabilityTelemetry"/> with full load.
	/// </summary>
	public AvailabilityTelemetry Create_AvailabilityTelemetry_Max()
	{
		var id = GetId();

		var duration = GetRandomDuration(100, 2000);

		var result = new AvailabilityTelemetry
		{
			Duration = duration,
			Id = id,
			Measurements = Measurements,
			Message = Message,
			Name = Name,
			Operation = Operation,
			Properties = Properties,
			RunLocation = "Earth",
			Success = true,
			Tags = Tags,
			Time = DateTime.UtcNow
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="AvailabilityTelemetry"/> with minimum load.
	/// </summary>
	public AvailabilityTelemetry Create_AvailabilityTelemetry_Min()
	{
		var id = GetId();

		var result = new AvailabilityTelemetry
		{
			Duration = TimeSpan.Zero,
			Id = id,
			Message = Message,
			Name = Name,
			Operation = Operation,
			Success = true,
			Time = DateTime.UtcNow
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="DependencyTelemetry"/> with full load.
	/// </summary>
	public DependencyTelemetry Create_DependencyTelemetry_Max()
	{
		var id = GetId();

		var type = DependencyType.DetectTypeFromHttp(Url);

		var duration = GetRandomDuration(200, 800);

		var result = new DependencyTelemetry
		{
			Data = "data",
			Duration = duration,
			Id = id,
			Measurements = Measurements,
			Name = Name,
			Operation = Operation,
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
	/// Creates instance of <see cref="DependencyTelemetry"/> with minimum load.
	/// </summary>
	public DependencyTelemetry Create_DependencyTelemetry_Min()
	{
		var id = GetId();

		var result = new DependencyTelemetry
		{
			Operation = Operation,
			Time = DateTime.UtcNow,
			Id = id,
			Name = Name
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="EventTelemetry"/> with full load.
	/// </summary>
	public EventTelemetry Create_EventTelemetry_Max()
	{
		var result = new EventTelemetry
		{
			Measurements = Measurements,
			Name = Name,
			Operation = Operation,
			Properties = Properties,
			Tags = Tags,
			Time = DateTime.UtcNow
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="EventTelemetry"/> with minimum load.
	/// </summary>
	public EventTelemetry Create_EventTelemetry_Min()
	{
		var result = new EventTelemetry
		{
			Operation = Operation,
			Time = DateTime.UtcNow,
			Name = Name
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="ExceptionTelemetry"/> with full load.
	/// </summary>
	public ExceptionTelemetry Create_ExceptionTelemetry_Max()
	{
		try
		{
			Simulate_ExceptionThrow_WithInnerException(null);

			throw new Exception();
		}
		catch (Exception exception)
		{
			var result = new ExceptionTelemetry
			{
				Exception = exception,
				Measurements = Measurements,
				Operation = Operation,
				Properties = Properties,
				SeverityLevel = SeverityLevel.Critical,
				Tags = Tags,
				Time = DateTime.UtcNow
			};

			return result;
		}
	}

	/// <summary>
	/// Creates instance of <see cref="ExceptionTelemetry"/> with minimum load.
	/// </summary>
	public ExceptionTelemetry Create_ExceptionTelemetry_Min()
	{
		try
		{
			Simulate_ExceptionThrow(null);

			throw new Exception();
		}
		catch (Exception exception)
		{
			var result = new ExceptionTelemetry
			{
				Exception = exception,
				Operation = Operation,
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
		Double value,
		MetricValueAggregation aggregation
	)
	{
		var result = new MetricTelemetry
		{
			Operation = Operation,
			Time = DateTime.UtcNow,
			Namespace = @namespace,
			Name = Name,
			Value = value,
			ValueAggregation = aggregation,
			Properties = Properties,
			Tags = Tags
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="MetricTelemetry"/> with minimum load.
	/// </summary>
	public MetricTelemetry Create_MetricTelemetry_Min
	(
		String @namespace,
		Double value
	)
	{
		var result = new MetricTelemetry
		{
			Name = Name,
			Namespace = @namespace,
			Operation = Operation,
			Time = DateTime.UtcNow,
			Value = value
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="PageViewTelemetry"/> with full load.
	/// </summary>
	public PageViewTelemetry Create_PageViewTelemetry_Max()
	{
		var id = GetId();

		var duration = GetRandomDuration(300, 700);

		var result = new PageViewTelemetry
		{
			Measurements = Measurements,
			Duration = duration,
			Id = id,
			Name = Name,
			Operation = Operation,
			Properties = Properties,
			Tags = Tags,
			Time = DateTime.UtcNow,
			Url = Url
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="PageViewTelemetry"/> with minimum load.
	/// </summary>
	public PageViewTelemetry Create_PageViewTelemetry_Min()
	{
		var id = GetId();

		var result = new PageViewTelemetry
		{
			Id = id,
			Name = Name,
			Operation = Operation,
			Time = DateTime.UtcNow
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="RequestTelemetry"/> with full load.
	/// </summary>
	public RequestTelemetry Create_RequestTelemetry_Max()
	{
		var id = GetId();

		var result = new RequestTelemetry
		{
			Duration = TimeSpan.FromSeconds(1),
			Id = id,
			Measurements = Measurements,
			Name = Name,
			Operation = Operation,
			Properties = Properties,
			ResponseCode = "200",
			Success = true,
			Tags = Tags,
			Time = DateTime.UtcNow,
			Url = Url
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="RequestTelemetry"/> with minimum load.
	/// </summary>
	public RequestTelemetry Create_RequestTelemetry_Min()
	{
		var id = GetId();

		var result = new RequestTelemetry
		{
			Operation = Operation,
			Time = DateTime.UtcNow,
			Id = id,
			Url = Url,
			ResponseCode = "1"
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="TraceTelemetry"/> with full load.
	/// </summary>
	public TraceTelemetry Create_TraceTelemetry_Max()
	{
		var result = new TraceTelemetry
		{
			Message = Message,
			Operation = Operation,
			Properties = Properties,
			SeverityLevel = SeverityLevel.Information,
			Tags = Tags,
			Time = DateTime.UtcNow
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="TraceTelemetry"/> with minimum load.
	/// </summary>
	public TraceTelemetry Create_TraceTelemetry_Min()
	{
		var result = new TraceTelemetry
		{
			Message = Message,
			Operation = Operation,
			SeverityLevel = SeverityLevel.Verbose,
			Time = DateTime.UtcNow
		};

		return result;
	}

	#endregion
}
