// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;

using Azure.Monitor.Telemetry.Code;

/// <summary>
/// Providing functionality to collect and publish telemetry items.
/// </summary>
/// <remarks>
/// Provides thread-safe collection of telemetry items and supports batch publishing through configured telemetry publishers.
/// Allows specifying common tags that will be attached to each telemetry item during publish operation.
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

	private static readonly TelemetryPublishResult[] emptySuccess = [];
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
	/// The distirbuted operation stored in asynchronous local storage.
	/// </summary>
	public TelemetryOperation Operation
	{
		get => operation.Value ?? TelemetryOperation.Empty;

		set => operation.Value = value;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Adds a telemetry item to the tracking queue.
	/// </summary>
	/// <param name="telemetry">The telemetry item to add.</param>
	public void Add(Telemetry telemetry)
	{
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

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	//public OperationStartInfo TrackOperationBegin
	//(
	//	DateTime time,
	//	Int64 timestamp,
	//	String id,
	//	String operationId,
	//	String? operationName = null,
	//	String? syntheticSource = null
	//)
	//{
	//	// get current operation
	//	var originalOperation = Operation;

	//	// replace operation
	//	Operation = new TelemetryOperation
	//	{
	//		Id = operationId,
	//		Name = operationName,
	//		ParentId = id,
	//		SyntheticSource = syntheticSource,
	//	};

	//	var result = new OperationStartInfo
	//	{
	//		Id = id,
	//		Operation = originalOperation,
	//		Time = time,
	//		Timestamp = timestamp
	//	};

	//	return result;
	//}

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	//public OperationStartInfo TrackOperationBegin
	//(
	//	Func<String> getId,
	//	String operationId,
	//	String? operationName = null,
	//	String? syntheticSource = null
	//)
	//{
	//	// get time
	//	var time = DateTime.UtcNow;

	//	// get timestamp to calculate duration on end
	//	var timestamp = Stopwatch.GetTimestamp();

	//	// get id
	//	var id = getId();

	//	// call overload
	//	var result = TrackOperationBegin(time, timestamp, id, operationId, operationName, syntheticSource);

	//	return result;
	//}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public OperationStartInfo TrackOperationBegin
	(
		DateTime time,
		Int64 timestamp,
		String id
	)
	{
		// get current operation
		var originalOperation = Operation;

		// replace operation
		Operation = new TelemetryOperation
		{
			Id = originalOperation.Id,
			Name = originalOperation.Name,
			ParentId = id
		};

		var result = new OperationStartInfo
		{
			Id = id,
			Operation = originalOperation,
			Time = time,
			Timestamp = timestamp
		};

		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public OperationStartInfo TrackOperationBegin
	(
		Func<String> getId
	)
	{
		// get time
		var time = DateTime.UtcNow;

		// get timestamp to calculate duration on end
		var timestamp = Stopwatch.GetTimestamp();

		// get id
		var id = getId();

		// call overload
		var result = TrackOperationBegin(time, timestamp, id);

		return result;
	}

	/// <summary>
	/// Begins a nested operation.
	/// </summary>
	/// <remarks>
	/// Replaces the <see cref="Operation"/> with a new object with parentId obtained with <paramref name="getId"/> call.
	/// In this way all subsequent telemetry will refer to the request as parent operation untill <see cref="TrackRequestEnd"/> called.
	/// </remarks>
	/// <param name="getId">A function that returns the identifier for the request.</param>
	/// <param name="previousParentId">The previous Operation parent identifier.</param>
	/// <param name="time">The timestamp when the tracking hes begun.</param>
	/// <param name="id">The generated identifier of the request.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void OperationBegin
	(
		Func<String> getId,
		out String? previousParentId,
		out DateTime time,
		out String id
	)
	{
		// set time
		time = DateTime.UtcNow;

		TrackRequestBegin(getId, out previousParentId, out id);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void OperationEnd
	(
		String? previousParentId,
		DateTime time,
		out TimeSpan duration
	)
	{
		// replace parent id
		Operation = Operation.CloneWithNewParentId(previousParentId);

		duration = DateTime.UtcNow - time;
	}

	#endregion

	#region Methods: Track

	/// <summary>
	/// Tracks an availability test result.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="AvailabilityTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the test was initiated.</param>
	/// <param name="id">The unique identifier.</param>
	/// <param name="name">The name of the telemetry instance.</param>
	/// <param name="message">The message associated with the telemetry instance.</param>
	/// <param name="duration">The time taken to complete the test.</param>
	/// <param name="success">A value indicating whether the operation was successful or unsuccessful.</param>
	/// <param name="runLocation">Location from where the test has been performed.  Is optional.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackAvailability
	(
		DateTime time,
		String id,
		String name,
		String message,
		TimeSpan duration,
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
	/// Tracks a dependency call.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="DependencyTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the dependency call was initiated.</param>
	/// <param name="id">The unique identifier.</param>
	/// <param name="httpMethod">The HTTP method used for the dependency call.</param>
	/// <param name="uri">The URI of the dependency call.</param>
	/// <param name="statusCode">The HTTP status code returned by the dependency call.</param>
	/// <param name="duration">The time taken to complete the dependency call.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackDependency
	(
		DateTime time,
		String id,
		HttpMethod httpMethod,
		Uri uri,
		HttpStatusCode statusCode,
		TimeSpan duration,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var name = String.Concat(httpMethod.Method, " ", uri.AbsolutePath);

		var success = (Int32)statusCode is >= 200 and < 300;

		var telemetry = new DependencyTelemetry
		{
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
			Data = uri.ToString(),
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
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackPageViewEnd
	(
		OperationStartInfo operationInfo,
		String name,
		Uri url,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		// replace operation with previous operation
		Operation = operationInfo.Operation;

		// var duration = new TimeSpan( (Stopwatch.GetTimestamp() - operationInfo.Timestamp) * Stopwatch.Frequency);
		var duration = DateTime.UtcNow - operationInfo.Time;

		var telemetry = new PageViewTelemetry
		{
			Duration = duration,
			Id = operationInfo.Id,
			Measurements = measurements,
			Name = name,
			Operation = operationInfo.Operation,
			Properties = properties,
			Tags = tags,
			Time = operationInfo.Time,
			Url = url
		};

		Add(telemetry);
	}

	/// <summary>
	/// Begins tracking the execution of a request.
	/// </summary>
	/// <remarks>
	/// Replaces the <see cref="Operation"/> with a new object with parentId obtained with <paramref name="getId"/> call.
	/// In this way all subsequent telemetry will refer to the request as parent operation.
	/// </remarks>
	/// <param name="getId">A function that returns the identifier for the request.</param>
	/// <param name="previousParentId">The previous Operation parent identifier.</param>
	/// <param name="id">The generated identifier of the request.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackRequestBegin
	(
		Func<String> getId,
		out String? previousParentId,
		out String id
	)
	{
		// get request id
		id = getId();

		// replace operation parent id with request id
		Operation = Operation.CloneWithNewParentId(id, out previousParentId);
	}

	/// <summary>
	/// Begins tracking the execution of a request.
	/// </summary>
	/// <remarks>
	/// Replaces the <see cref="Operation"/> with a new object with parentId obtained with <paramref name="getId"/> call.
	/// In this way all subsequent telemetry will refer to the request as parent operation untill <see cref="TrackRequestEnd"/> called.
	/// </remarks>
	/// <param name="getId">A function that returns the identifier for the request.</param>
	/// <param name="previousParentId">The previous Operation parent identifier.</param>
	/// <param name="time">The timestamp when the tracking hes begun.</param>
	/// <param name="id">The generated identifier of the request.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackRequestBegin
	(
		Func<String> getId,
		out String? previousParentId,
		out DateTime time,
		out String id
	)
	{
		// set time
		time = DateTime.UtcNow;

		TrackRequestBegin(getId, out previousParentId, out id);
	}

	/// <summary>
	/// Ends tracking the execution of a request.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="RequestTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="previousParentId">The identifier of previous parent to replace the operation's parent identifier with.</param>
	/// <param name="time">The UTC timestamp when the request was initiated.</param>
	/// <param name="id">The unique identifier.</param>
	/// <param name="url">The request url.</param>
	/// <param name="responseCode">The result of an operation execution.</param>
	/// <param name="success">A value indicating whether the operation was successful or unsuccessful.</param>
	/// <param name="duration">The time taken to complete.</param>
	/// <param name="name">The name of the request. Is optional.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackRequestEnd
	(
		String? previousParentId,
		DateTime time,
		String id,
		Uri url,
		String responseCode,
		Boolean success,
		TimeSpan duration,
		String? name = null,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		// replace operation with in-proc dependency as parent id 
		Operation = Operation.CloneWithNewParentId(previousParentId);

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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackRequestEnd
	(
		OperationStartInfo operationInfo,
		Uri url,
		String responseCode,
		Boolean success,
		String? name = null,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		// replace operation with previous operation
		Operation = operationInfo.Operation;

		// var duration = new TimeSpan( (Stopwatch.GetTimestamp() - operationInfo.Timestamp) * Stopwatch.Frequency);
		var duration = DateTime.UtcNow - operationInfo.Time;

		var telemetry = new RequestTelemetry
		{
			Duration = duration,
			Id = operationInfo.Id,
			Measurements = measurements,
			Name = name,
			Operation = Operation,
			Properties = properties,
			ResponseCode = responseCode,
			Success = success,
			Tags = tags,
			Time = operationInfo.Time,
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

	public void TrackAvailabilityEnd
	(
		OperationStartInfo operationInfo,
		String name,
		String message,
		Boolean success,
		String? runLocation = null,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		// replace operation with previous operation
		Operation = operationInfo.Operation;

		var duration = TimeSpan.FromMilliseconds( (Int32) (DateTime.UtcNow - operationInfo.Time).TotalMilliseconds );

		var propertyValue = new KeyValuePair<String, String>("FullTestResultAvailable", "false");

		IReadOnlyList<KeyValuePair<String, String>> propertiesAdjusted = properties == null ? [propertyValue] : [propertyValue, ..properties];

		var telemetry = new AvailabilityTelemetry
		{
			Duration = duration,
			Id = operationInfo.Id,
			Measurements = measurements,
			Message = message,
			Name = name,
			Operation = Operation,
			Properties = propertiesAdjusted,
			RunLocation = runLocation,
			Success = success,
			Tags = tags,
			Time = operationInfo.Time
		};

		Add(telemetry);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackDependencyEnd
	(
		OperationStartInfo operationInfo,
		HttpMethod httpMethod,
		Uri uri,
		HttpStatusCode statusCode,
		Boolean success,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		// replace operation with previous operation
		Operation = operationInfo.Operation;

		// var duration = new TimeSpan( (Stopwatch.GetTimestamp() - operationInfo.Timestamp) * Stopwatch.Frequency);
		var duration = DateTime.UtcNow - operationInfo.Time;

		var name = String.Concat(httpMethod.Method, " ", uri.AbsolutePath);

		var telemetry = new DependencyTelemetry
		{
			Data = uri.ToString(),
			Duration = duration,
			Id = operationInfo.Id,
			Measurements = measurements,
			Name = name,
			Operation = Operation,
			Properties = properties,
			ResultCode = statusCode.ToString(),
			Success = success,
			Tags = tags,
			Target = uri.Host,
			Type = DependencyType.DetectTypeFromHttp(uri),
			Time = operationInfo.Time
		};

		Add(telemetry);
	}

	/// <summary>
	/// Ends tracking of an InProc dependency.
	/// </summary>
	/// <remarks>
	/// Reverts back <see cref="Operation"/> as it was before calling <see cref="TrackDependencyInProcBegin(Func{String}, out String?, out DateTime, out String)"/>
	/// Creates an instance of <see cref="DependencyTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the dependency call was initiated.</param>
	/// <param name="id">The unique identifier.</param>
	/// <param name="name">The name of the command initiated the dependency call.</param>
	/// <param name="success">A value indicating whether the operation was successful or unsuccessful.</param>
	/// <param name="duration">The time taken to complete the dependency call.</param>
	/// <param name="typeName">The type name of the dependency. Is optional.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackDependencyInProcEnd
	(
		OperationStartInfo operationInfo,
		String name,
		Boolean success,
		String? typeName = null,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		// replace operation with previous operation
		Operation = operationInfo.Operation;

		var duration = DateTime.UtcNow - operationInfo.Time;

		var type = String.IsNullOrWhiteSpace(typeName) ? DependencyType.InProc : DependencyType.InProc + " | " + typeName;

		var telemetry = new DependencyTelemetry
		{
			Duration = duration,
			Id = operationInfo.Id,
			Measurements = measurements,
			Name = name,
			Operation = Operation,
			Properties = properties,
			Success = success,
			Tags = tags,
			Type = type,
			Time = operationInfo.Time
		};

		Add(telemetry);
	}
}
