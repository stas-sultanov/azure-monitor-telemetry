// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.Telemetry;

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;

using Azure.Monitor.Telemetry.Models;

/// <summary>
/// Provides functionality to collect and publish telemetry data.
/// </summary>
/// <remarks>
/// Utilizes <see cref="ConcurrentQueue{T}"/> to collect telemetry items and publish in a batch through configured telemetry publishers.
/// </remarks>
public sealed class TelemetryClient
{
	#region Types

	/// <summary>
	/// A structure that holds the telemetry tags and its representation in list form.
	/// </summary>
	/// <remarks>This type allows to reduce number of expensive <see cref="TelemetryTags.ToArray()"/> calls.</remarks>
	/// <param name="tags">The telemetry tags.</param>
	private readonly struct ContextTuple(TelemetryTags tags)
	{
		public KeyValuePair<String, String>[]? AsArray { get; } = tags.IsEmpty() ? null : tags.ToArray();

		public TelemetryTags Collection { get; } = tags;
	}

	#endregion

	#region Static Fields

	private static readonly TelemetryPublishResult[] emptyPublishResult = [];

	#endregion

	#region Fields

	private readonly AsyncLocal<ContextTuple> localContext;
	private readonly ConcurrentQueue<Telemetry> items;
	private readonly TelemetryPublisher[] publishers;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="TelemetryClient"/> class.
	/// </summary>
	/// <param name="publisher">A telemetry publisher to publish the telemetry data.</param>
	/// <param name="tags">The tags to initialize the context.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="publisher"/> is null.</exception>
	public TelemetryClient
	(
		TelemetryPublisher publisher,
		TelemetryTags? tags = null
	)
	{
		if (publisher is null)
		{
			throw new ArgumentNullException(nameof(publisher));
		}

		tags ??= TelemetryTags.Empty;

		localContext = new()
		{
			Value = new(tags)
		};

		items = new();

		publishers = [publisher];
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TelemetryClient"/> class.
	/// </summary>
	/// <param name="publishers">A read only list of telemetry publishers to publish the telemetry data.</param>
	/// <param name="tags">The tags to initialize the context.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="publishers"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="publishers"/> count is 0.</exception>
	/// <exception cref="ArgumentException">Thrown if any publisher in <paramref name="publishers"/> is null.</exception>
	public TelemetryClient
	(
		IReadOnlyList<TelemetryPublisher> publishers,
		TelemetryTags? tags = null
	)
	{
		if (publishers is null)
		{
			throw new ArgumentNullException(nameof(publishers));
		}

		if (publishers.Count == 0)
		{
			throw new ArgumentException("The list of publishers is empty.", nameof(publishers));
		}

		for (var index = 0; index < publishers.Count; index++)
		{
			if (publishers[index] is null)
			{
				throw new ArgumentException($"The publisher at index {index} is null.", nameof(publishers));
			}
		}

		tags ??= TelemetryTags.Empty;

		localContext = new()
		{
			Value = new(tags)
		};

		items = new();

		this.publishers = [.. publishers];
	}

	#endregion

	#region Properties

	/// <summary>
	/// A read-only list of tags to add to <see cref="Telemetry.Tags"/> property of each telemetry item with Track* method.
	/// </summary>
	public TelemetryTags Context
	{
		get => localContext.Value.Collection;

		set => localContext.Value = new ContextTuple(value);
	}

	#endregion

	#region Methods

	/// <summary>
	/// Adds a telemetry item into the tracking queue.
	/// </summary>
	/// <param name="telemetry">The telemetry item to add.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add
	(
		Telemetry telemetry
	)
	{
		items.Enqueue(telemetry);
	}

	/// <summary>
	/// Publishes telemetry items asynchronously to all configured telemetry publishers.
	/// </summary>
	/// <remarks>
	/// This method dequeues all telemetry items from the internal queue and publishes them to all configured 
	/// telemetry publishers. If there are no items to publish, it returns an empty result array.
	/// </remarks>
	/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains an array of <see cref="TelemetryPublishResult"/> indicating the result of the publish operation for each publisher.
	/// </returns>
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
		var telemetryItems = new List<Telemetry>();

		// dequeue all items from the queue
		while (items.TryDequeue(out var telemetry))
		{
			telemetryItems.Add(telemetry);
		}

		// create a list to store publish results
		var resultList = new Task<TelemetryPublishResult>[publishers.Length];

		// publish telemetry items via all configured publishers
		for (var publisherIndex = 0; publisherIndex < publishers.Length; publisherIndex++)
		{
			var publisher = publishers[publisherIndex];

			resultList[publisherIndex] = publisher.PublishAsync(telemetryItems, cancellationToken);
		}

		// wait for all publishers to complete
		var result = await Task.WhenAll(resultList);

		return result;
	}

	#endregion

	#region Methods: Scope

	/// <summary>
	/// Begins an activity scope.
	/// </summary>
	/// <remarks>
	/// The method replaces <see cref="Context"/> with new value where <see cref="TelemetryTagKeys.OperationParentId"/> is set to the <paramref name="activityId"/>.
	/// All telemetry items tracked within the scope will have <paramref name="activityId"/> as parent activity identifier.
	/// </remarks>
	/// <param name="activityId">The activity unique identifier to use as parent for telemetry items tracked within the scope.</param>
	/// <param name="context">Outputs a value of <see cref="Context"/> before it is replaced.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ActivityScopeBegin
	(
		String activityId,
		out TelemetryTags context
	)
	{
		// set current context
		context = Context;

		Context = context is null
			? new TelemetryTags()
			{
				OperationParentId = activityId
			}
			: (context with
			{
				OperationParentId = activityId
			});
	}

	/// <summary>
	/// Begins an activity scope.
	/// </summary>
	/// <remarks>
	/// The method replaces <see cref="Context"/> with new value where <see cref="TelemetryTagKeys.OperationParentId"/> is set to the <paramref name="activityId"/>.
	/// All telemetry items tracked within the scope will have <paramref name="activityId"/> as parent activity identifier.
	/// </remarks>
	/// <param name="getActivityId">A function that returns a unique identifier for the activity.</param>
	/// <param name="time">Outputs the UTC timestamp when the activity scope begins.</param>
	/// <param name="timestamp">Outputs the timestamp to calculate the duration when the activity scope ends.</param>
	/// <param name="activityId">Outputs the generated unique identifier for the activity.</param>
	/// <param name="context">Outputs a value of <see cref="Context"/> before it is replaced.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ActivityScopeBegin
	(
		Func<String> getActivityId,
		out DateTime time,
		out Int64 timestamp,
		out String activityId,
		out TelemetryTags context
	)
	{
		// get time
		time = DateTime.UtcNow;

		// get timestamp to calculate duration on end
		timestamp = Stopwatch.GetTimestamp();

		// get id
		activityId = getActivityId();

		// call overload
		ActivityScopeBegin(activityId, out context);
	}

	/// <summary>
	/// Ends the current activity scope.
	/// </summary>
	/// <remarks>
	/// The method restores value of <see cref="Context"/> with <paramref name="context"/>.
	/// </remarks>
	/// <param name="context">The telemetry context to be restored.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ActivityScopeEnd
	(
		TelemetryTags context
	)
	{
		// bring back context
		Context = context;
	}

	/// <summary>
	/// Ends the current activity scope and calculates duration of the activity.
	/// </summary>
	/// <remarks>
	/// The method restores value of <see cref="Context"/> with <paramref name="context"/>.
	/// </remarks>
	/// <param name="context">The telemetry operation that is being tracked.</param>
	/// <param name="timestamp">The timestamp when the operation started.</param>
	/// <param name="duration">The output parameter that will hold the duration of the operation.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ActivityScopeEnd
	(
		TelemetryTags context,
		Int64 timestamp,
		out TimeSpan duration
	)
	{
		// call overload
		ActivityScopeEnd(context);

		// get timestamp to calculate duration
		var endTimestamp = Stopwatch.GetTimestamp();

		// calculate duration in ticks
		var durationInTicks = (endTimestamp - timestamp) * TimeSpan.TicksPerSecond / Stopwatch.Frequency;

		// set duration
		duration = new TimeSpan(durationInTicks);
	}

	#endregion

	#region Methods: Track

	/// <summary>
	/// Tracks an availability test activity.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="AvailabilityTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the activity was initiated.</param>
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
		var contextTags = localContext.Value.AsArray;

		var telemetryTags = tags is null ? contextTags : (contextTags is null ? tags : [..contextTags, ..tags]);

		var telemetry = new AvailabilityTelemetry
		{
			Duration = duration,
			Id = id,
			Measurements = measurements,
			Message = message,
			Name = name,
			Properties = properties,
			RunLocation = runLocation,
			Success = success,
			Tags = telemetryTags,
			Time = time
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks a dependency activity.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="DependencyTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the activity was initiated.</param>
	/// <param name="duration">The time taken to complete the activity.</param>
	/// <param name="id">The unique identifier of the activity.</param>
	/// <param name="name">The name of the command initiated the dependency call.</param>
	/// <param name="success">A value indicating whether the operation was successful or unsuccessful.</param>
	/// <param name="data">The command initiated by this dependency call.</param>
	/// <param name="target">This field is the target site of a dependency call.</param>
	/// <param name="type">The dependency type name.</param>
	/// <param name="resultCode">The result of executing SQL command.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackDependency
	(
		DateTime time,
		TimeSpan duration,
		String id,
		String name,
		Boolean success,
		String? resultCode = null,
		String? data = null,
		String? target = null,
		String? type = null,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var contextTags = localContext.Value.AsArray;

		var telemetryTags = tags is null ? contextTags : (contextTags is null ? tags : [..contextTags, ..tags]);

		var telemetry = new DependencyTelemetry
		{
			Data = data,
			Duration = duration,
			Id = id,
			Measurements = measurements,
			Name = name,
			Properties = properties,
			ResultCode = resultCode,
			Success = success,
			Tags = telemetryTags,
			Target = target,
			Type = type,
			Time = time
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks an HTTP dependency call activity.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="DependencyTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the activity was initiated.</param>
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
	public void TrackDependencyHttp
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
		var data = uri.ToString();

		var name = String.Concat(httpMethod.Method, " ", uri.AbsolutePath);

		var resultCode = statusCode.ToString();

		var target = uri.Host;

		var type = TelemetryUtils.DetectDependencyTypeFromHttpUri(uri);

		TrackDependency
		(
			time,
			duration,
			id,
			name,
			success,
			resultCode,
			data,
			target,
			type,
			measurements,
			properties,
			tags
		);
	}

	/// <summary>
	/// Tracks an in-proc dependency activity.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="DependencyTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the activity was initiated.</param>
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
		var type = String.IsNullOrWhiteSpace(typeName) ? DependencyTypes.InProc : DependencyTypes.InProc + " | " + typeName;

		TrackDependency
		(
			time,
			duration,
			id,
			name,
			success,
			type: type,
			measurements: measurements,
			properties: properties,
			tags: tags
		);
	}

	/// <summary>
	/// Tracks a SQL call dependency activity.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="DependencyTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the activity was initiated.</param>
	/// <param name="duration">The time taken to complete the activity.</param>
	/// <param name="id">The unique identifier of the activity.</param>
	/// <param name="dataSource">The name of the instance of SQL Server. Often FQDN of the server.</param>
	/// <param name="database">The name of the database.</param>
	/// <param name="commandText">The text of SQL command.</param>
	/// <param name="resultCode">The result of executing SQL command.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackDependencySql
	(
		DateTime time,
		TimeSpan duration,
		String id,
		String dataSource,
		String database,
		String commandText,
		Int32 resultCode,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var dataFullName = String.Concat(dataSource, " | ", database);

		var resultCodeAsString = resultCode == 0 ? null : resultCode.ToString(CultureInfo.InvariantCulture);

		var success = resultCode >= 0;

		TrackDependency
		(
			time,
			duration,
			id,
			dataFullName,
			success,
			resultCodeAsString,
			commandText,
			dataFullName,
			DependencyTypes.SQL,
			measurements,
			properties,
			tags
		);
	}

	/// <summary>
	/// Tracks an event.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="EventTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the event has occurred.</param>
	/// <param name="name">The name.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackEvent
	(
		DateTime time,
		String name,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var contextTags = localContext.Value.AsArray;

		var telemetryTags = tags is null ? contextTags : (contextTags is null ? tags : [..contextTags, ..tags]);

		var telemetry = new EventTelemetry
		{
			Measurements = measurements,
			Name = name,
			Properties = properties,
			Tags = telemetryTags,
			Time = time
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks an event.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="EventTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
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

		TrackEvent(time, name, measurements, properties, tags);
	}

	/// <summary>
	/// Tracks an exception.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="ExceptionTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the exception has occurred.</param>
	/// <param name="exception">The exception to be tracked.</param>
	/// <param name="problemId">The problem identifier.</param>
	/// <param name="severityLevel">The severity level of the exception. Is optional.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackException
	(
		DateTime time,
		Exception exception,
		String? problemId = null,
		SeverityLevel? severityLevel = null,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var exceptions = TelemetryUtils.ConvertExceptionToModel(exception);

		var contextTags = localContext.Value.AsArray;

		var telemetryTags = tags is null ? contextTags : (contextTags is null ? tags : [..contextTags, ..tags]);

		var telemetry = new ExceptionTelemetry
		{
			Exceptions = exceptions,
			Measurements = measurements,
			Properties = properties,
			ProblemId = problemId,
			SeverityLevel = severityLevel,
			Tags = telemetryTags,
			Time = time
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks an exception.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="ExceptionTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="exception">The exception to be tracked.</param>
	/// <param name="problemId">The problem identifier.</param>
	/// <param name="severityLevel">The severity level of the exception. Is optional.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackException
	(
		Exception exception,
		String? problemId = null,
		SeverityLevel? severityLevel = null,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var time = DateTime.UtcNow;

		TrackException(time, exception, problemId, severityLevel, measurements, properties, tags);
	}

	/// <summary>
	/// Tracks a metric.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="MetricTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the metric was recorded.</param>
	/// <param name="namespace">The namespace of the metric to be tracked.</param>
	/// <param name="name">The name of the metric to be tracked.</param>
	/// <param name="value">The value of the metric to be tracked.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackMetric
	(
		DateTime time,
		String @namespace,
		String name,
		Double value,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var contextTags = localContext.Value.AsArray;

		var telemetryTags = tags is null ? contextTags : (contextTags is null ? tags : [..contextTags, ..tags]);

		var telemetry = new MetricTelemetry
		{
			Namespace = @namespace,
			Name = name,
			Properties = properties,
			Tags = telemetryTags,
			Time = time,
			Value = value
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks a metric.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="MetricTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="namespace">The namespace of the metric to be tracked.</param>
	/// <param name="name">The name of the metric to be tracked.</param>
	/// <param name="value">The value of the metric to be tracked.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackMetric
	(
		String @namespace,
		String name,
		Double value,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var time = DateTime.UtcNow;

		TrackMetric(time, @namespace, name, value, properties, tags);
	}

	/// <summary>
	/// Tracks a metric.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="MetricTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the metric was recorded.</param>
	/// <param name="namespace">The namespace of the metric to be tracked.</param>
	/// <param name="name">The name of the metric to be tracked.</param>
	/// <param name="value">The value of the metric to be tracked.</param>
	/// <param name="count">The number of values in the sample set.</param>
	/// <param name="max">The max value of the metric across the sample set.</param>
	/// <param name="min">The min value of the metric across the sample set.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackMetric
	(
		DateTime time,
		String @namespace,
		String name,
		Double value,
		Int32 count,
		Double max,
		Double min,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var valueAggregation = new MetricValueAggregation()
		{
			Count = count,
			Max = max,
			Min = min
		};

		var contextTags = localContext.Value.AsArray;

		var telemetryTags = tags is null ? contextTags : (contextTags is null ? tags : [..contextTags, ..tags]);

		var telemetry = new MetricTelemetry
		{
			Namespace = @namespace,
			Name = name,
			Properties = properties,
			Tags = telemetryTags,
			Time = time,
			Value = value,
			ValueAggregation = valueAggregation
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks a metric.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="MetricTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="namespace">The namespace of the metric to be tracked.</param>
	/// <param name="name">The name of the metric to be tracked.</param>
	/// <param name="value">The value of the metric to be tracked.</param>
	/// <param name="count">The number of values in the sample set.</param>
	/// <param name="max">The max value of the metric across the sample set.</param>
	/// <param name="min">The min value of the metric across the sample set.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackMetric
	(
		String @namespace,
		String name,
		Double value,
		Int32 count,
		Double max,
		Double min,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var time = DateTime.UtcNow;

		TrackMetric(time, @namespace, name, value, count, max, min, properties, tags);
	}

	/// <summary>
	/// Tracks a page view activity.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="PageViewTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the activity was initiated.</param>
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
		var contextTags = localContext.Value.AsArray;

		var telemetryTags = tags is null ? contextTags : (contextTags is null ? tags : [..contextTags, ..tags]);

		var telemetry = new PageViewTelemetry
		{
			Duration = duration,
			Id = id,
			Measurements = measurements,
			Name = name,
			Properties = properties,
			Tags = telemetryTags,
			Time = time,
			Url = url
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks a request activity.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="RequestTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the activity was initiated.</param>
	/// <param name="duration">The time taken to complete the activity.</param>
	/// <param name="id">The unique identifier of the activity.</param>
	/// <param name="url">The URL of the request.</param>
	/// <param name="responseCode">The response code of the request.</param>
	/// <param name="success">Indicates whether the request was successful.</param>
	/// <param name="name">Optional. The name of the request.</param>
	/// <param name="source">The source of the request.</param>
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
		String? source = null,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var contextTags = localContext.Value.AsArray;

		var telemetryTags = tags is null ? contextTags : (contextTags is null ? tags : [..contextTags, ..tags]);

		var telemetry = new RequestTelemetry
		{
			Duration = duration,
			Id = id,
			Measurements = measurements,
			Name = name,
			Properties = properties,
			ResponseCode = responseCode,
			Source = source,
			Success = success,
			Tags = telemetryTags,
			Time = time,
			Url = url
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks a trace.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="TraceTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
	/// </remarks>
	/// <param name="time">The UTC timestamp when the trace has occurred.</param>
	/// <param name="message">The message.</param>
	/// <param name="severityLevel">The severity level.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TrackTrace
	(
		DateTime time,
		String message,
		SeverityLevel severityLevel,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var contextTags = localContext.Value.AsArray;

		var telemetryTags = tags is null ? contextTags : (contextTags is null ? tags : [..contextTags, ..tags]);

		var telemetry = new TraceTelemetry
		{
			Message = message,
			Properties = properties,
			SeverityLevel = severityLevel,
			Tags = telemetryTags,
			Time = time
		};

		Add(telemetry);
	}

	/// <summary>
	/// Tracks a trace.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="TraceTelemetry"/> using <see cref="Context"/> and calls the <see cref="Add(Telemetry)"/> method.
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

		TrackTrace(time, message, severityLevel, properties, tags);
	}

	#endregion
}
