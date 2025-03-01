// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides functionality to collect and publish telemetry data.
/// </summary>
/// <remarks>
/// Utilizes <see cref="ConcurrentQueue{T}"/> to collect telemetry items and publish in a batch through configured telemetry publishers.
/// </remarks>
/// <param name="telemetryPublishers">A read only list of telemetry publishers to publish the telemetry data.</param>
/// <param name="tags">A read-only list of tags to attach to each telemetry item. Is optional.</param>
public sealed class TelemetryTracker
(
	IReadOnlyList<TelemetryPublisher> telemetryPublishers,
	IReadOnlyList<KeyValuePair<String, String>>? tags = null
)
{
	#region Fields

	private static readonly TelemetryPublishResult[] emptyPublishResult = [];
	private readonly ConcurrentQueue<Telemetry> items = new();
	private readonly IReadOnlyList<KeyValuePair<String, String>>? tags = tags;
	private readonly AsyncLocal<TelemetryOperation> operation = new();
	private readonly IReadOnlyList<TelemetryPublisher> telemetryPublishers = telemetryPublishers;

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="TelemetryTracker"/> class.
	/// </summary>
	/// <param name="telemetryPublisher">A telemetry publisher to publish the telemetry data.</param>
	/// <param name="tags">A read-only list of tags to attach to each telemetry item. Is optional.</param>
	public TelemetryTracker
	(
		TelemetryPublisher telemetryPublisher,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
		: this([telemetryPublisher], tags)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// The distributed operation stored in asynchronous local storage.
	/// </summary>
	public TelemetryOperation Operation
	{
		get => operation.Value!;

		set => operation.Value = value;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Adds a telemetry item into the tracking queue.
	/// </summary>
	/// <param name="telemetry">The telemetry item to add.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(Telemetry telemetry)
	{
		items.Enqueue(telemetry);
	}

	/// <summary>
	/// Publishes in parallel all telemetry items from the queue using all configured telemetry publishers.
	/// </summary>
	/// <param name="cancellationToken">Optional token to cancel the operation.</param>
	/// <returns>An array of <see cref="TelemetryPublishResult"/> indicating the status for each configured publisher.</returns>
	/// <remarks>
	/// If the queue is empty, returns an empty success result array.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public async Task<TelemetryPublishResult[]> PublishAsync
	(
		CancellationToken cancellationToken = default
	)
	{
		// check if there are any items to publish
		if (items.IsEmpty)
		{
			return emptyPublishResult;
		}

		// create a list to store telemetry items
		// List.Capacity do not use ConcurrentQueue.Count because it takes time to calculate and it may change 
		var telemetryList = new List<Telemetry>();

		// dequeue all items from the queue
		while (items.TryDequeue(out var telemetry))
		{
			telemetryList.Add(telemetry);
		}

		// create a list to store publish results
		var resultList = new Task<TelemetryPublishResult>[telemetryPublishers.Count];

		// publish telemetry items to all configured senders
		for (var publisherIndex = 0; publisherIndex < telemetryPublishers.Count; publisherIndex++)
		{
			var sender = telemetryPublishers[publisherIndex];

			resultList[publisherIndex] = sender.PublishAsync(telemetryList, tags, cancellationToken);
		}

		// wait for all publishers to complete
		var result = await Task.WhenAll(resultList);

		return result;
	}

	#endregion

	#region Methods: Operation

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ActivityScopeBegin
	(
		String id,
		out TelemetryOperation operation
	)
	{
		// get current operation
		operation = Operation;

		// replace operation with new parent activity id
		Operation = new TelemetryOperation
		{
			Id = operation.Id,
			Name = operation.Name,
			ParentId = id
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ActivityScopeBegin
	(
		Func<String> getId,
		out DateTime time,
		out Int64 timestamp,
		out String id,
		out TelemetryOperation operation
	)
	{
		// get time
		time = DateTime.UtcNow;

		// get timestamp to calculate duration on end
		timestamp = Stopwatch.GetTimestamp();

		// get id
		id = getId();

		// call overload
		ActivityScopeBegin(id, out operation);
	}

	/// <summary>
	/// Begins tracking of an operation.
	/// </summary>
	/// <param name="getId">The function to get telemetry identifier.</param>
	/// <returns>An object that represents a context of operation tracking.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TimeSpan ActivityScopeEnd
	(
		Int64 timestamp,
		TelemetryOperation operation
	)
	{
		// get timestamp to calculate duration
		var endTimestamp = Stopwatch.GetTimestamp();

		// bring back operation
		Operation = operation;

		// calculate duration
		TimeSpan result = new ((endTimestamp - timestamp) * TimeSpan.TicksPerSecond / Stopwatch.Frequency);

		return result;
	}

	#endregion

	#region Methods: Track

	/// <summary>
	/// Taracks an availability test activity.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="AvailabilityTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the activity was intiated.</param>
	/// <param name="duration">The time taken to complete the activity.</param>
	/// <param name="id">The unique identifier of the activity.</param>
	/// <param name="name">The name of the availability test.</param>
	/// <param name="message">A message describing the result of the availability test.</param>
	/// <param name="success">A boolean indicating whether the availability test was successful.</param>
	/// <param name="runLocation">The location where the availability test was run. Optional.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackAvailability
	(
		DateTime time,
		TimeSpan duration,
		String id,
		String name,
		String message,
		Boolean success,
		String? runLocation = null,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var telemetry = new AvailabilityTelemetry
		{
			Duration = duration,
			Id = id,
			Measurements = measurements,
			Message = message,
			Name = name,
			Operation = Operation,
			Properties = properties,
			RunLocation = runLocation,
			Success = success,
			Tags = tags,
			Time = time
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks a dependency call activity.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="DependencyTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the activity was intiated.</param>
	/// <param name="duration">The time taken to complete the activity.</param>
	/// <param name="id">The unique identifier of the activity.</param>
	/// <param name="httpMethod">The HTTP method used in the operation.</param>
	/// <param name="uri">The URI of the dependency.</param>
	/// <param name="statusCode">The HTTP status code returned by the dependency.</param>
	/// <param name="success">Indicates whether the dependency call was successful.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackDependency
	(
		DateTime time,
		TimeSpan duration,
		String id,
		HttpMethod httpMethod,
		Uri uri,
		HttpStatusCode statusCode,
		Boolean success,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var name = String.Concat(httpMethod.Method, " ", uri.AbsolutePath);

		var telemetry = new DependencyTelemetry
		{
			Data = uri.ToString(),
			Duration = duration,
			Id = id,
			Measurements = measurements,
			Name = name,
			Operation = Operation,
			Properties = properties,
			ResultCode = statusCode.ToString(),
			Success = success,
			Tags = tags,
			Target = uri.Host,
			Type = DependencyType.DetectTypeFromHttp(uri),
			Time = time
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks an in-proc dependency activity.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="DependencyTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the activity was intiated.</param>
	/// <param name="duration">The time taken to complete the activity.</param>
	/// <param name="id">The unique identifier of the activity.</param>
	/// <param name="name">The name of the dependency operation.</param>
	/// <param name="success">Indicates whether the operation was successful.</param>
	/// <param name="typeName">The type name of the dependency operation. Optional.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackDependencyInProc
	(
		DateTime time,
		TimeSpan duration,
		String id,
		String name,
		Boolean success,
		String? typeName = null,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var type = String.IsNullOrWhiteSpace(typeName) ? DependencyType.InProc : DependencyType.InProc + " | " + typeName;

		var telemetry = new DependencyTelemetry
		{
			Duration = duration,
			Id = id,
			Measurements = measurements,
			Name = name,
			Operation = Operation,
			Properties = properties,
			Success = success,
			Tags = tags,
			Type = type,
			Time = time
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks an event.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="EventTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="name">The name.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackEvent
	(
		String name,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var time = DateTime.UtcNow;

		var telemetry = new EventTelemetry
		{
			Measurements = measurements,
			Name = name,
			Operation = Operation,
			Properties = properties,
			Tags = tags,
			Time = time
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks an exception.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="ExceptionTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="exception">The exception to be tracked.</param>
	/// <param name="severityLevel">The severity level of the exception. Is optional.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackException
	(
		Exception exception,
		SeverityLevel? severityLevel = null,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var time = DateTime.UtcNow;

		var telemetry = new ExceptionTelemetry
		{
			Exception = exception,
			Measurements = measurements,
			Operation = Operation,
			Properties = properties,
			SeverityLevel = severityLevel,
			Tags = tags,
			Time = time
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks a metric.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="MetricTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="namespace">The namespace of the metric to be tracked.</param>
	/// <param name="name">The name of the metric to be tracked.</param>
	/// <param name="value">The value of the metric to be tracked.</param>
	/// <param name="valueAggregation">The aggregation type of the metric. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackMetric
	(
		String @namespace,
		String name,
		Double value,
		MetricValueAggregation? valueAggregation = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var time = DateTime.UtcNow;

		var telemetry = new MetricTelemetry
		{
			Namespace = @namespace,
			Name = name,
			Operation = Operation,
			Properties = properties,
			Tags = tags,
			Time = time,
			Value = value,
			ValueAggregation = valueAggregation
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks a page view activity.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="PageViewTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the activity was intiated.</param>
	/// <param name="duration">The time taken to complete the activity.</param>
	/// <param name="id">The unique identifier of the activity.</param>
	/// <param name="name">The name of the page view.</param>
	/// <param name="url">The URL of the page view.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackPageView
	(
		DateTime time,
		TimeSpan duration,
		String id,
		String name,
		Uri url,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var telemetry = new PageViewTelemetry
		{
			Duration = duration,
			Id = id,
			Measurements = measurements,
			Name = name,
			Operation = Operation,
			Properties = properties,
			Tags = tags,
			Time = time,
			Url = url
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks a request activity.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="RequestTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the activity was intiated.</param>
	/// <param name="duration">The time taken to complete the activity.</param>
	/// <param name="id">The unique identifier of the activity.</param>
	/// <param name="url">The URL of the request.</param>
	/// <param name="responseCode">The response code of the request.</param>
	/// <param name="success">Indicates whether the request was successful.</param>
	/// <param name="name">Optional. The name of the request.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackRequest
	(
		DateTime time,
		TimeSpan duration,
		String id,
		Uri url,
		String responseCode,
		Boolean success,
		String? name = null,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var telemetry = new RequestTelemetry
		{
			Duration = duration,
			Id = id,
			Measurements = measurements,
			Name = name,
			Operation = Operation,
			Properties = properties,
			ResponseCode = responseCode,
			Success = success,
			Tags = tags,
			Time = time,
			Url = url
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks a trace.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="TraceTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="message">The message.</param>
	/// <param name="severityLevel">The severity level.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackTrace
	(
		String message,
		SeverityLevel severityLevel,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var time = DateTime.UtcNow;

		var telemetry = new TraceTelemetry
		{
			Message = message,
			Operation = Operation,
			Properties = properties,
			SeverityLevel = severityLevel,
			Tags = tags,
			Time = time
		};

		Add(telemetry);
	}

	#endregion
}
