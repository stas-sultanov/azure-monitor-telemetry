// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;

/// <summary>
/// Providing functionality to collect and publish telemetry items.
/// </summary>
/// <remarks>
/// Provides thread-safe collection of telemetry items and supports batch publishing
/// through configured telemetry publishers. It allows attaching common tags that will be included with all telemetry items.
/// </remarks>
/// <param name="tags">A list of tags to attach to each telemetry item. Is optional.</param>
/// <param name="telemetryPublishers">An array of telemetry publishers to publish the telemetry data.</param>
public sealed class TelemetryTracker
(
	TagList tags = null,
	params TelemetryPublisher[] telemetryPublishers
)
{
	#region Fields

	private static readonly TelemetryPublishResult[] emptySuccess = [];
	private readonly ConcurrentQueue<Telemetry> items = new();
	private readonly TagList tags = tags;
	private readonly AsyncLocal<OperationContext> operation = new();
	private readonly TelemetryPublisher[] telemetryPublishers = telemetryPublishers;

	#endregion

	#region Properties

	/// <summary>
	/// An asynchronous local storage for the current telemetry operation.
	/// </summary>
	public OperationContext Operation
	{
		get => operation.Value;

		set => operation.Value = value;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Adds a telemetry item to the tracking queue, if it is not <c>null</c>.
	/// </summary>
	/// <param name="telemetry">The telemetry item to add.</param>
	public void Add(Telemetry telemetry)
	{
		if (telemetry == null)
		{
			return;
		}

		items.Enqueue(telemetry);
	}

	/// <summary>
	/// Publishes all telemetry items in the queue using all configured telemetry publishers.
	/// </summary>
	/// <param name="cancellationToken">Optional token to cancel the operation.</param>
	/// <returns>An array of <see cref="TelemetryPublishResult"/> indicating the status for each configured publisher.</returns>
	/// <remarks>
	/// If the queue is empty, returns an empty success result array.
	/// The method processes all items in the queue and publishes them in parallel to all configured publishers.
	/// </remarks>
	public async Task<TelemetryPublishResult[]> PublishAsync
	(
		CancellationToken cancellationToken = default
	)
	{
		// check if there are any items to publish
		if (items.IsEmpty)
		{
			return emptySuccess;
		}

		// create a list to store telemetry items
		var telemetryList = new List<Telemetry>(items.Count * 2); // just for case of active adding while flush

		// dequeue all items from the queue
		while (items.TryDequeue(out var telemetry))
		{
			telemetryList.Add(telemetry);
		}

		// create a list to store publish results
		var resultList = new Task<TelemetryPublishResult>[telemetryPublishers.Length];

		// publish telemetry items to all configured senders
		for (var publisherIndex = 0; publisherIndex < telemetryPublishers.Length; publisherIndex++)
		{
			var sender = telemetryPublishers[publisherIndex];

			resultList[publisherIndex] = sender.PublishAsync(telemetryList, tags, cancellationToken);
		}

		// wait for all publishers to complete
		var result = await Task.WhenAll(resultList);

		return result;
	}

	#endregion

	#region Methods: Track

	/// <summary>
	/// Tracks availability by creating an instance of <see cref="AvailabilityTelemetry"/> and calling the <see cref="Add(Telemetry)"/> method.
	/// </summary>
	/// <param name="time">The time when the dependency call was initiated.</param>
	/// <param name="id">The unique identifier for the dependency call.</param>
	/// <param name="name">The name of the telemetry instance.</param>
	/// <param name="message">The message associated with the telemetry instance.</param>
	/// <param name="duration">The duration of the dependency call.</param>
	/// <param name="success">A boolean indicating whether the dependency call was successful.</param>
	/// <param name="runLocation">The location where the telemetry was run. This parameter is optional.</param>
	/// <param name="measurements">The measurements associated with the telemetry. This parameter is optional.</param>
	/// <param name="properties">The properties associated with the telemetry. This parameter is optional.</param>
	/// <param name="tags">The tags associated with the telemetry. This parameter is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackAvailability
	(
		DateTime time,
		String id,
		String name,
		String message,
		TimeSpan duration,
		Boolean success,
		String runLocation = null,
		MeasurementList measurements = null,
		PropertyList properties = null,
		TagList tags = null
	)
	{
		var telemetry = new AvailabilityTelemetry(time, id, name, message)
		{
			Duration = duration,
			Measurements = measurements,
			Operation = Operation,
			Properties = properties,
			RunLocation = runLocation,
			Success = success,
			Tags = tags
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks dependency by creating an instance of <see cref="DependencyTelemetry"/> and calling the <see cref="Add(Telemetry)"/> method.
	/// </summary>
	/// <param name="time">The time when the dependency call was initiated.</param>
	/// <param name="id">The unique identifier for the dependency call.</param>
	/// <param name="httpMethod">The HTTP method used for the dependency call.</param>
	/// <param name="uri">The URI of the dependency call.</param>
	/// <param name="statusCode">The HTTP status code returned by the dependency call.</param>
	/// <param name="duration">The duration of the dependency call.</param>
	/// <param name="measurements">The measurements associated with the telemetry. This parameter is optional.</param>
	/// <param name="properties">The properties associated with the telemetry. This parameter is optional.</param>
	/// <param name="tags">The tags associated with the telemetry. This parameter is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackDependency
	(
		DateTime time,
		String id,
		HttpMethod httpMethod,
		Uri uri,
		HttpStatusCode statusCode,
		TimeSpan duration,
		MeasurementList measurements = null,
		PropertyList properties = null,
		TagList tags = null
	)
	{
		var name = String.Concat(httpMethod.Method, " ", uri.AbsolutePath);

		var success = (Int32)statusCode is >= 200 and < 300;

		var telemetry = new DependencyTelemetry(time, id, name)
		{
			Duration = duration,
			Measurements = measurements,
			Operation = Operation,
			Properties = properties,
			ResultCode = statusCode.ToString(),
			Success = success,
			Tags = tags,
			Target = uri.Host,
			Type = DependencyType.DetectTypeFromHttp(uri),
			Data = uri.ToString()
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks InProc dependency by creating an instance of <see cref="DependencyTelemetry"/> and calling the <see cref="Add(Telemetry)"/> method.
	/// </summary>
	/// <param name="time">The time at which the dependency call was made.</param>
	/// <param name="id">The unique identifier for the dependency call.</param>
	/// <param name="name">The name of the dependency.</param>
	/// <param name="success">A boolean indicating whether the dependency call was successful.</param>
	/// <param name="duration">The duration of the dependency call.</param>
	/// <param name="typeName">The type name of the dependency. Defaults to null.</param>
	/// <param name="measurements">A list of measurements associated with the dependency call. Defaults to null.</param>
	/// <param name="properties">A list of properties associated with the dependency call. Defaults to null.</param>
	/// <param name="tags">A list of tags associated with the dependency call. Defaults to null.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackDependencyInProc
	(
		DateTime time,
		String id,
		String name,
		Boolean success,
		TimeSpan duration,
		String typeName = null,
		MeasurementList measurements = null,
		PropertyList properties = null,
		TagList tags = null
	)
	{
		var type = String.IsNullOrWhiteSpace(typeName) ? DependencyType.InProc : DependencyType.InProc + " | " + typeName;

		var telemetry = new DependencyTelemetry(time, id, name)
		{
			Duration = duration,
			Measurements = measurements,
			Operation = Operation,
			Properties = properties,
			Success = success,
			Tags = tags,
			Type = type
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks event by creating an instance of <see cref="EventTelemetry"/> and calling the <see cref="Add(Telemetry)"/> method.
	/// </summary>
	/// <param name="name">The name of the event to be tracked.</param>
	/// <param name="measurements">The measurements associated with the telemetry. This parameter is optional.</param>
	/// <param name="properties">The properties associated with the telemetry. This parameter is optional.</param>
	/// <param name="tags">The tags associated with the telemetry. This parameter is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackEvent
	(
		String name,
		MeasurementList measurements = null,
		PropertyList properties = null,
		TagList tags = null
	)
	{
		var time = DateTime.UtcNow;

		var telemetry = new EventTelemetry(time, name)
		{
			Measurements = measurements,
			Operation = Operation,
			Properties = properties,
			Tags = tags
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks exception by creating an instance of <see cref="ExceptionTelemetry"/> and calling the <see cref="Add(Telemetry)"/> method.
	/// </summary>
	/// <param name="exception">The exception to be tracked.</param>
	/// <param name="measurements">The measurements associated with the telemetry. This parameter is optional.</param>
	/// <param name="properties">The properties associated with the telemetry. This parameter is optional.</param>
	/// <param name="severityLevel">The severity level of the exception. This parameter is optional.</param>
	/// <param name="tags">The tags associated with the telemetry. This parameter is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackException
	(
		Exception exception,
		SeverityLevel? severityLevel = null,
		MeasurementList measurements = null,
		PropertyList properties = null,
		TagList tags = null
	)
	{
		var time = DateTime.UtcNow;

		var telemetry = new ExceptionTelemetry(time, exception)
		{
			Measurements = measurements,
			Operation = Operation,
			Properties = properties,
			SeverityLevel = severityLevel,
			Tags = tags
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks metric by creating an instance of <see cref="MetricTelemetry"/> and calling the <see cref="Add(Telemetry)"/> method.
	/// </summary>
	/// <param name="namespace">The namespace of the metric to be tracked.</param>
	/// <param name="name">The name of the metric to be tracked.</param>
	/// <param name="value">The value of the metric to be tracked.</param>
	/// <param name="valueAggregation">The aggregation type of the metric. This parameter is optional.</param>
	/// <param name="properties">The properties associated with the telemetry. This parameter is optional.</param>
	/// <param name="tags">The tags associated with the telemetry. This parameter is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackMetric
	(
		String @namespace,
		String name,
		Double value,
		MetricValueAggregation valueAggregation = null,
		PropertyList properties = null,
		TagList tags = null
	)
	{
		var time = DateTime.UtcNow;

		var telemetry = new MetricTelemetry(time, @namespace, name, value, valueAggregation)
		{
			Operation = Operation,
			Properties = properties,
			Tags = tags
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks trace by creating an instance of <see cref="TraceTelemetry"/> and calling the <see cref="Add(Telemetry)"/> method.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <param name="severityLevel">The severity level.</param>
	/// <param name="properties">The properties associated with the telemetry. This parameter is optional.</param>
	/// <param name="tags">The tags associated with the telemetry. This parameter is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackTrace
	(
		String message,
		SeverityLevel severityLevel,
		PropertyList properties = null,
		TagList tags = null
	)
	{
		var time = DateTime.UtcNow;

		var telemetry = new TraceTelemetry(time, message, severityLevel)
		{
			Operation = Operation,
			Properties = properties,
			Tags = tags
		};

		Add(telemetry);
	}

	#endregion
}
