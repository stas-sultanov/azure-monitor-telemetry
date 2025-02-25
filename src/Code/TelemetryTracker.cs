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
/// Provides thread-safe collection of telemetry items and supports batch publishing through configured telemetry publishers.
/// Allows specifying common tags that will be attached to each telemetry item during publish operation.
/// </remarks>
public sealed class TelemetryTracker
{
	#region Fields

	private static readonly TelemetryPublishResult[] emptySuccess = [];
	private readonly ConcurrentQueue<Telemetry> items;
	private readonly KeyValuePair<String, String>[]? tags;
	private readonly AsyncLocal<OperationContext> operation;
	private readonly OperationContext rootOperation;
	private readonly TelemetryPublisher[] telemetryPublishers;

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="TelemetryTracker"/> class.
	/// </summary>
	/// <param name="telemetryPublisher">A telemetry publisher to publish the telemetry data.</param>
	/// <param name="rootOperation">Root distributed operation. Is optional.</param>
	/// <param name="tags">An array of tags to attach to each telemetry item. Is optional.</param>
	public TelemetryTracker
	(
		TelemetryPublisher telemetryPublisher,
		OperationContext? rootOperation = null,
		KeyValuePair<String, String>[]? tags = null
	)
		: this([telemetryPublisher], rootOperation, tags)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TelemetryTracker"/> class.
	/// </summary>
	/// <param name="telemetryPublishers">An array of telemetry publishers to publish the telemetry data.</param>
	/// <param name="rootOperation">Root distributed operation.</param>
	/// <param name="tags">An array of tags to attach to each telemetry item. Is optional.</param>
	public TelemetryTracker
	(
		TelemetryPublisher[] telemetryPublishers,
		OperationContext? rootOperation = null,
		KeyValuePair<String, String>[]? tags = null
	)
	{
		this.telemetryPublishers = telemetryPublishers;

		this.rootOperation = rootOperation ?? new OperationContext();

		this.tags = tags;

		items = new();

		operation = new()
		{
			Value = this.rootOperation
		};
	}

	#endregion

	#region Properties

	/// <summary>
	/// An asynchronous local storage for the current telemetry operation.
	/// </summary>
	public OperationContext Operation
	{
		get => operation.Value ?? rootOperation;

		private set => operation.Value = value;
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
	/// Tracks availability test result.
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
	/// <param name="measurements">An array of key-value pairs representing measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">An array of key-value pairs representing properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">An array of key-value pairs representing tags associated with the telemetry. Is optional.</param>	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackAvailability
	(
		DateTime time,
		String id,
		String name,
		String message,
		TimeSpan duration,
		Boolean success,
		String? runLocation = null,
		KeyValuePair<String, Double>[]? measurements = null,
		KeyValuePair<String, String>[]? properties = null,
		KeyValuePair<String, String>[]? tags = null
	)
	{
		var telemetry = new AvailabilityTelemetry(Operation, time, id, name, message)
		{
			Duration = duration,
			Measurements = measurements,
			Properties = properties,
			RunLocation = runLocation,
			Success = success,
			Tags = tags
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks dependency call.
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
	/// <param name="measurements">An array of key-value pairs representing measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">An array of key-value pairs representing properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">An array of key-value pairs representing tags associated with the telemetry. Is optional.</param>	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackDependency
	(
		DateTime time,
		String id,
		HttpMethod httpMethod,
		Uri uri,
		HttpStatusCode statusCode,
		TimeSpan duration,
		KeyValuePair<String, Double>[]? measurements = null,
		KeyValuePair<String, String>[]? properties = null,
		KeyValuePair<String, String>[]? tags = null
	)
	{
		var name = String.Concat(httpMethod.Method, " ", uri.AbsolutePath);

		var success = (Int32)statusCode is >= 200 and < 300;

		var telemetry = new DependencyTelemetry(Operation, time, id, name)
		{
			Duration = duration,
			Measurements = measurements,
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
	/// Begins tracking of an InProc dependency execution.
	/// </summary>
	/// <remarks>
	/// Replaces the <see cref="Operation"/> with a new object with parentId obtained with <paramref name="getId"/> call.
	/// In this way all subsequent telemetry will refer to the InProc dependency execution as parent operation.
	/// </remarks>
	/// <param name="getId">A function that returns the identifier for in-process dependency.</param>
	/// <param name="previousParentId">The previous Operation parent identifier.</param>
	/// <param name="time">The timestamp when the tracking hes begun.</param>
	/// <param name="id">The generated identifier of in-process dependency.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackDependencyInProcBegin
	(
		Func<String> getId,
		out String? previousParentId,
		out DateTime time,
		out String id
	)
	{
		// set time
		time = DateTime.UtcNow;

		// get in-proc dependency id
		id = getId();

		// replace operation with in-proc dependency as parent id 
		Operation = Operation.CloneWithNewParentId(id, out previousParentId);
	}

	/// <summary>
	/// Ends tracking of the InProc dependency.
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
	/// <param name="measurements">An array of key-value pairs representing measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">An array of key-value pairs representing properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">An array of key-value pairs representing tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackDependencyInProcEnd
	(
		String? previousParentId,
		DateTime time,
		String id,
		String name,
		Boolean success,
		TimeSpan duration,
		String? typeName = null,
		KeyValuePair<String, Double>[]? measurements = null,
		KeyValuePair<String, String>[]? properties = null,
		KeyValuePair<String, String>[]? tags = null
	)
	{
		// replace operation with in-proc dependency as parent id 
		Operation = Operation.CloneWithNewParentId(previousParentId);

		var type = String.IsNullOrWhiteSpace(typeName) ? DependencyType.InProc : DependencyType.InProc + " | " + typeName;

		var telemetry = new DependencyTelemetry(Operation, time, id, name)
		{
			Duration = duration,
			Measurements = measurements,
			Properties = properties,
			Success = success,
			Tags = tags,
			Type = type
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks event.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="EventTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="name">The name.</param>
	/// <param name="measurements">An array of key-value pairs representing measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">An array of key-value pairs representing properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">An array of key-value pairs representing tags associated with the telemetry. Is optional.</param>	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackEvent
	(
		String name,
		KeyValuePair<String, Double>[]? measurements = null,
		KeyValuePair<String, String>[]? properties = null,
		KeyValuePair<String, String>[]? tags = null
	)
	{
		var time = DateTime.UtcNow;

		var telemetry = new EventTelemetry(Operation, time, name)
		{
			Measurements = measurements,
			Properties = properties,
			Tags = tags
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks exception.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="ExceptionTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="exception">The exception to be tracked.</param>
	/// <param name="severityLevel">The severity level of the exception. Is optional.</param>
	/// <param name="measurements">An array of key-value pairs representing measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">An array of key-value pairs representing properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">An array of key-value pairs representing tags associated with the telemetry. Is optional.</param>	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackException
	(
		Exception exception,
		SeverityLevel? severityLevel = null,
		KeyValuePair<String, Double>[]? measurements = null,
		KeyValuePair<String, String>[]? properties = null,
		KeyValuePair<String, String>[]? tags = null
	)
	{
		var time = DateTime.UtcNow;

		var telemetry = new ExceptionTelemetry(Operation, time, exception)
		{
			Measurements = measurements,
			Properties = properties,
			SeverityLevel = severityLevel,
			Tags = tags
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks metric.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="MetricTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="namespace">The namespace of the metric to be tracked.</param>
	/// <param name="name">The name of the metric to be tracked.</param>
	/// <param name="value">The value of the metric to be tracked.</param>
	/// <param name="valueAggregation">The aggregation type of the metric. Is optional.</param>
	/// <param name="properties">An array of key-value pairs representing properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">An array of key-value pairs representing tags associated with the telemetry. Is optional.</param
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackMetric
	(
		String @namespace,
		String name,
		Double value,
		MetricValueAggregation? valueAggregation = null,
		KeyValuePair<String, String>[]? properties = null,
		KeyValuePair<String, String>[]? tags = null
	)
	{
		var time = DateTime.UtcNow;

		var telemetry = new MetricTelemetry(Operation, time, @namespace, name, value, valueAggregation)
		{
			Properties = properties,
			Tags = tags
		};

		Add(telemetry);
	}

	/// <summary>
	/// Begins tracking of an request execution.
	/// </summary>
	/// <remarks>
	/// Replaces the <see cref="Operation"/> with a new object with parentId obtained with <paramref name="getId"/> call.
	/// In this way all subsequent telemetry will refer to the request as parent operation.
	/// </remarks>
	/// <param name="getId">A function that returns the identifier for request.</param>
	/// <param name="previousParentId">The previous Operation parent identifier.</param>
	/// <param name="time">The timestamp when the tracking hes begun.</param>
	/// <param name="id">The generated identifier of in-process dependency.</param>
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

		// get in-proc dependency id
		id = getId();

		// replace operation with in-proc dependency as parent id 
		Operation = Operation.CloneWithNewParentId(id, out previousParentId);
	}

	/// <summary>
	/// Ends tracking of the request dependency.
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
	/// <param name="measurements">An array of key-value pairs representing measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">An array of key-value pairs representing properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">An array of key-value pairs representing tags associated with the telemetry. Is optional.</param>
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
		KeyValuePair<String, Double>[]? measurements = null,
		KeyValuePair<String, String>[]? properties = null,
		KeyValuePair<String, String>[]? tags = null
	)
	{
		// replace operation with in-proc dependency as parent id 
		Operation = Operation.CloneWithNewParentId(previousParentId);

		var telemetry = new RequestTelemetry(Operation, time, id, url, responseCode)
		{
			Duration = duration,
			Measurements = measurements,
			Name = name,
			Properties = properties,
			Success = success,
			Tags = tags
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks trace.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="TraceTelemetry"/> using <see cref="Operation"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="message">The message.</param>
	/// <param name="severityLevel">The severity level.</param>
	/// <param name="properties">An array of key-value pairs representing properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">An array of key-value pairs representing tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackTrace
	(
		String message,
		SeverityLevel severityLevel,
		KeyValuePair<String, String>[]? properties = null,
		KeyValuePair<String, String>[]? tags = null
	)
	{
		var time = DateTime.UtcNow;

		var telemetry = new TraceTelemetry(Operation, time, message, severityLevel)
		{
			Properties = properties,
			Tags = tags
		};

		Add(telemetry);
	}

	#endregion
}
