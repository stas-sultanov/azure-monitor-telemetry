// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Publish;

using System;
using System.Net.Http;
using System.Runtime.CompilerServices;

using Azure.Monitor.Telemetry;

/// <summary>
/// Provides extension methods for the <see cref="TelemetryClient"/> class.
/// </summary>
public static class TelemetryClientExtensions
{
	#region Methods

	/// <summary>
	/// Tracks an instance of <see cref="HttpTelemetryPublishResult"/> as dependency telemetry.
	/// </summary>
	/// <param name="telemetryClient">The telemetry tracker instance.</param>
	/// <param name="id">The unique identifier of the activity.</param>
	/// <param name="publishResult">The result of the publish operation.</param>
	/// <param name="measurements">A read-only list of measurements associated with the telemetry. Is optional.</param>
	/// <param name="properties">A read-only list of properties associated with the telemetry. Is optional.</param>
	/// <param name="tags">A read-only list of tags associated with the telemetry. Is optional.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void TrackDependency
	(
		this TelemetryClient telemetryClient,
		String id,
		HttpTelemetryPublishResult publishResult,
		IReadOnlyList<KeyValuePair<String, Double>>? measurements = null,
		IReadOnlyList<KeyValuePair<String, String>>? properties = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		var countMeasurement = new KeyValuePair<String, Double>(nameof(HttpTelemetryPublishResult.Count), publishResult.Count);

		KeyValuePair<String, Double>[] measurementsWithCount = measurements == null ? [countMeasurement] : [..measurements, countMeasurement];

		telemetryClient.TrackDependencyHttp
		(
			publishResult.Time,
			publishResult.Duration,
			id,
			HttpMethod.Post,
			publishResult.Url,
			publishResult.StatusCode,
			publishResult.Success,
			measurementsWithCount,
			properties,
			tags
		);
	}

	#endregion
}
