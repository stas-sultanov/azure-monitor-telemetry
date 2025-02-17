// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Publish;

/// <summary>
/// Represents the response from an HTTP tracking operation in version 2 format.
/// </summary>
/// <param name="appId">The unique identifier for the application.</param>
/// <param name="errors">An array of errors that occurred during processing. Each error contains the index, message, and status code.</param>
/// <param name="itemsAccepted">The number of items that were successfully processed and accepted.</param>
/// <param name="itemsReceived">The total number of items that were received in the request.</param>
public sealed class HttpTelemetryPublishResponse
(
	String appId,
	HttpTelemetryPublishError[] errors,
	UInt16 itemsAccepted,
	UInt16 itemsReceived
)
{
	#region Properties

	/// <summary>
	/// The unique identifier for the application.
	/// </summary>
	public String AppId { get; } = appId;

	/// <summary>
	/// The array of errors associated with the HTTP response.
	/// </summary>
	public HttpTelemetryPublishError[] Errors { get; } = errors;

	/// <summary>
	/// The number of items that were successfully accepted and processed.
	/// </summary>
	public UInt16 ItemsAccepted { get; } = itemsAccepted;

	/// <summary>
	/// The number of items received by the service.
	/// </summary>
	public UInt16 ItemsReceived { get; } = itemsReceived;

	#endregion
}
