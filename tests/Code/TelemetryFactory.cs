// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Tests;

using System;

/// <summary>
/// Provides instances of classes that implements <see cref="Telemetry"/> for testing purposes.
/// </summary>
internal sealed class TelemetryFactory
{
	#region Properties

	private Random Random { get; }
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
		(
			id: Guid.NewGuid().ToString("N"),
			name: "Test #" + DateTime.Now.ToString("yyMMddhhmm"),
			syntheticSource: "TestFramework"
		);

		Properties = [new("key", "value")];

		Random = new Random(DateTime.UtcNow.Millisecond);

		Tags = [new(TelemetryTagKey.CloudRole, "TestMachine")];

		Url = new Uri("https://gostas.dev");
	}

	#endregion

	#region Methods: Helpers

	internal static String GetId()
	{
		return Guid.NewGuid().ToString("N").Substring(0, 16);
	}

	internal TimeSpan GetRandomDuration(Int32 millisecondsMin, Int32 millisecondsMax)
	{
		var milliseconds = Random.Next(millisecondsMin, millisecondsMax);

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

		var result = new AvailabilityTelemetry(Operation, DateTime.UtcNow, id, Name, Message)
		{
			Duration = duration,
			Measurements = Measurements,
			Properties = Properties,
			RunLocation = "Earth",
			Success = true,
			Tags = Tags,
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="AvailabilityTelemetry"/> with minimum load.
	/// </summary>
	public AvailabilityTelemetry Create_AvailabilityTelemetry_Min()
	{
		var id = GetId();

		var result = new AvailabilityTelemetry(Operation, DateTime.UtcNow, id, Name, Message);

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

		var result = new DependencyTelemetry(Operation, DateTime.UtcNow, id, Name)
		{
			Measurements = Measurements,
			Data = "data",
			Duration = duration,
			Properties = Properties,
			Success = false,
			Tags = Tags,
			ResultCode = "401",
			Type = type,
			Target = "target"
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="DependencyTelemetry"/> with minimum load.
	/// </summary>
	public DependencyTelemetry Create_DependencyTelemetry_Min()
	{
		var id = GetId();

		var result = new DependencyTelemetry(Operation, DateTime.UtcNow, id, Name);

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="EventTelemetry"/> with full load.
	/// </summary>
	public EventTelemetry Create_EventTelemetry_Max()
	{
		var result = new EventTelemetry(Operation, DateTime.UtcNow, Name)
		{
			Measurements = Measurements,
			Properties = Properties,
			Tags = Tags
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="EventTelemetry"/> with minimum load.
	/// </summary>
	public EventTelemetry Create_EventTelemetry_Min()
	{
		var result = new EventTelemetry(Operation, DateTime.UtcNow, Name);

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
			var result = new ExceptionTelemetry(Operation, DateTime.UtcNow, exception)
			{
				Measurements = Measurements,
				Properties = Properties,
				SeverityLevel = SeverityLevel.Critical,
				Tags = Tags
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
			var result = new ExceptionTelemetry(Operation, DateTime.UtcNow, exception);

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
		var result = new MetricTelemetry(Operation, DateTime.UtcNow, @namespace, Name, value, aggregation)
		{
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
		var result = new MetricTelemetry(Operation, DateTime.UtcNow, @namespace, Name, value);

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="PageViewTelemetry"/> with full load.
	/// </summary>
	public PageViewTelemetry Create_PageViewTelemetry_Max()
	{
		var id = GetId();

		var duration = GetRandomDuration(300, 700);

		var result = new PageViewTelemetry(Operation, DateTime.UtcNow, id, Name)
		{
			Measurements = Measurements,
			Duration = duration,
			Properties = Properties,
			Tags = Tags,
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

		var result = new PageViewTelemetry(Operation, DateTime.UtcNow, id, Name);

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="RequestTelemetry"/> with full load.
	/// </summary>
	public RequestTelemetry Create_RequestTelemetry_Max()
	{
		var id = GetId();

		var result = new RequestTelemetry(Operation, DateTime.UtcNow, id, Url, "200")
		{
			Measurements = Measurements,
			Name = Name,
			Duration = TimeSpan.FromSeconds(1),
			Properties = Properties,
			Success = true,
			Tags = Tags
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="RequestTelemetry"/> with minimum load.
	/// </summary>
	public RequestTelemetry Create_RequestTelemetry_Min()
	{
		var id = GetId();

		var result = new RequestTelemetry(Operation, DateTime.UtcNow, id, Url, "1");

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="TraceTelemetry"/> with full load.
	/// </summary>
	public TraceTelemetry Create_TraceTelemetry_Max()
	{
		var result = new TraceTelemetry(Operation, DateTime.UtcNow, Message, SeverityLevel.Information)
		{
			Properties = Properties,
			Tags = Tags
		};

		return result;
	}

	/// <summary>
	/// Creates instance of <see cref="TraceTelemetry"/> with minimum load.
	/// </summary>
	public TraceTelemetry Create_TraceTelemetry_Min()
	{
		var result = new TraceTelemetry(Operation, DateTime.UtcNow, Message, SeverityLevel.Verbose);

		return result;
	}

	#endregion
}
