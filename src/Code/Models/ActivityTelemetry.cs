// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Models;

/// <summary>
/// A contract for telemetry types that represent activity.
/// </summary>
public interface ActivityTelemetry : Telemetry
{
	#region Properties

	/// <summary>
	/// The time taken to complete the activity.
	/// </summary>
	public TimeSpan Duration { get; }

	/// <summary>
	/// The unique identifier of the activity.
	/// </summary>
	/// <remarks>Maximum length: 512 characters.</remarks>
	public String Id { get; }

	#endregion
}
