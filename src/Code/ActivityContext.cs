// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

using System;

/// <summary>
/// Represents the context for tracking telemetry data.
/// </summary>
public sealed class ActivityContext
{
	/// <summary>
	/// The unique identifier of the telemetry item.
	/// </summary>
	public required String Id { get; init; }

	/// <summary>
	/// Information about the the parent operation.
	/// </summary>
	public required TelemetryOperation OriginalOperation { get; init; }

	/// <summary>
	/// The time when the operation has been initiated.
	/// </summary>
	public required DateTime Time { get; init; }

	/// <summary>
	/// The timestamp when the operation has been initiated.
	/// </summary>
	/// <remarks>
	/// The current number of ticks in the timer mechanism.
	/// </remarks>
	public required Int64 Timestamp { get; init; }
}