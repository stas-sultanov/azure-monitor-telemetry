// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

using System;

/// <summary>
/// Represents a contract for result of a telemetry Publish operation.
/// </summary>
public interface TelemetryPublishResult
{
	#region Properties

	/// <summary>
	/// The number of items transferred.
	/// </summary>
	public Int32 Count { get; }

	/// <summary>
	/// The duration of the telemetry transfer operation.
	/// </summary>
	public TimeSpan Duration { get; }

	/// <summary>
	/// A boolean value indicating whether the telemetry transfer operation was successful.
	/// </summary>
	public Boolean Success { get; }

	/// <summary>
	/// The time when telemetry transfer operation was initiated.
	/// </summary>
	public DateTime Time { get; }

	#endregion
}
