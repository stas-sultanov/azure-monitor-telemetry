// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Publish;

using System.Net;

/// <summary>
/// Encapsulates the outcome of publishing telemetry over HTTP.
/// </summary>
/// <param name="count">The number of telemetry items processed in this operation.</param>
/// <param name="duration">The time taken to complete the telemetry tracking operation.</param>
/// <param name="success">A value indicating whether the operation was successful.</param>
/// <param name="time">The timestamp when the operation was performed.</param>
/// <param name="url">The ingestion URL used for the telemetry operation.</param>
/// <param name="statusCode">The HTTP status code received from the telemetry endpoint.</param>
/// <param name="response">The raw response string received from the telemetry endpoint.</param>
public sealed class HttpTelemetryPublishResult
(
	Int32 count,
	TimeSpan duration,
	Boolean success,
	DateTime time,
	Uri url,
	HttpStatusCode statusCode,
	String response
)
	: TelemetryPublishResult
{
	#region Properties

	/// <inheritdoc/>
	public Int32 Count { get; } = count;

	/// <inheritdoc/>
	public TimeSpan Duration { get; } = duration;

	/// <summary>
	/// The raw response string received from the telemetry endpoint.
	/// </summary>
	public String Response { get; } = response;

	/// <summary>
	/// The HTTP status code received from the telemetry endpoint.
	/// </summary>
	public HttpStatusCode StatusCode { get; } = statusCode;

	/// <inheritdoc/>
	public Boolean Success { get; } = success;

	/// <inheritdoc/>
	public DateTime Time { get; } = time;

	/// <summary>
	/// The ingestion URL used for the telemetry operation.
	/// </summary>
	public Uri Url { get; } = url;

	#endregion
}
