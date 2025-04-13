// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

using System;

/// <summary>
/// A contract for types that represents a result of a telemetry Publish operation.
/// </summary>
public interface TelemetryPublishResult
{
	#region Properties

	/// <summary>
	/// The number of items transferred.
	/// </summary>
	public Int32 Count { get; }

	/// <summary>
	/// The duration of the telemetry Publish operation.
	/// </summary>
	public TimeSpan Duration { get; }

	/// <summary>
	/// A boolean value indicating whether the telemetry Publish operation was successful.
	/// </summary>
	public Boolean Success { get; }

	/// <summary>
	/// The time when telemetry Publish operation was initiated.
	/// </summary>
	public DateTime Time { get; }

	#endregion
}
