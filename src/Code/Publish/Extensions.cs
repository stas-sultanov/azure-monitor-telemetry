// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Publish;

using System;
using System.Runtime.CompilerServices;

using Azure.Monitor.Telemetry;

/// <summary>
/// Provides extension methods for the <see cref="TelemetryTracker"/> class.
/// </summary>
public static class Extensions
{
	#region Methods

	/// <summary>
	/// Tracks an instance of <see cref="HttpTelemetryPublishResult"/> as dependency telemetry.
	/// </summary>
	/// <param name="telemetryTracker">The telemetry tracker instance.</param>
	/// <param name="id">The unique identifier of the activity.</param>
	/// <param name="publishResult">The result of the publish operation.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void TrackDependency
	(
		this TelemetryTracker telemetryTracker,
		String id,
		HttpTelemetryPublishResult publishResult,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var countMeasurement = new KeyValuePair<String, Double>(nameof(HttpTelemetryPublishResult.Count), publishResult.Count);

		KeyValuePair<String, Double>[] measurementsWithCount = measurements == null ? [countMeasurement] : [..measurements, countMeasurement];

		var name = String.Concat("POST ", publishResult.Url.AbsolutePath);

		var telemetry = new DependencyTelemetry
		{
			Data = publishResult.Url.ToString(),
			Duration = publishResult.Duration,
			Id = id,
			Measurements = measurementsWithCount,
			Name = name,
			Operation = telemetryTracker.Operation,
			Properties = properties,
			ResultCode = publishResult.StatusCode.ToString(),
			Success = publishResult.Success,
			Target = publishResult.Url.Host,
			Tags = tags,
			Type = DependencyType.AzureMonitor,
			Time = publishResult.Time
		};

		telemetryTracker.Add(telemetry);
	}

	#endregion
}
