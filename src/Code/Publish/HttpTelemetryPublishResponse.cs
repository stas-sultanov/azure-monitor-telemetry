﻿// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Publish;

/// <summary>
/// Represents the response from an HTTP tracking operation in version 2 format.
/// </summary>
public sealed class HttpTelemetryPublishResponse
{
	#region Properties

	/// <summary>
	/// The array of errors associated with the HTTP response.
	/// </summary>
	public required HttpTelemetryPublishError[] Errors { get; init; }

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
