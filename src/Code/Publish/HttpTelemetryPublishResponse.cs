// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Publish;

/// <summary>
/// Represents a response of the Publish operation.
/// </summary>
public sealed class HttpTelemetryPublishResponse
{
	#region Properties

	/// <summary>
	/// The array of errors associated with the HTTP response.
	/// </summary>
	public required IReadOnlyList<HttpTelemetryPublishError> Errors { get; init; }

	/// <summary>
	/// The number of items that were successfully accepted and processed.
	/// </summary>
	public required UInt16 ItemsAccepted { get; init; }

	/// <summary>
	/// The number of items received by the service.
	/// </summary>
	public required UInt16 ItemsReceived { get; init; }

	#endregion
}
