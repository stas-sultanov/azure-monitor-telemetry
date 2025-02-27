// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Publish;

using System.Net;

/// <summary>
/// Encapsulates the outcome of publishing telemetry over HTTP.
/// </summary>
public sealed class HttpTelemetryPublishResult : TelemetryPublishResult
{
	#region Properties

	/// <inheritdoc/>
	public required Int32 Count { get; init; }

	/// <inheritdoc/>
	public required TimeSpan Duration { get; init; }

	/// <summary>
	/// The raw response string received from the telemetry endpoint.
	/// </summary>
	public required String Response { get; init; }

	/// <summary>
	/// The HTTP status code received from the telemetry endpoint.
	/// </summary>
	public required HttpStatusCode StatusCode { get; init; }

	/// <inheritdoc/>
	public required Boolean Success { get; init; }

	/// <inheritdoc/>
	public required DateTime Time { get; init; }

	/// <summary>
	/// The ingestion URL used for the telemetry operation.
	/// </summary>
	public required Uri Url { get; init; }

	#endregion
}
