// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Represents a base class for all types of telemetry data.
/// </summary>
public interface Telemetry
{
	#region Properties

	/// <summary>
	/// The details about the telemetry operation being performed.
	/// </summary>
	public TelemetryOperation Operation { get; }

	/// <summary>
	/// A read-only list of custom properties in a name-value format.
	/// This collection is used to extend standard telemetry data with custom dimensions.
	/// </summary>
	/// <remarks>
	/// Maximum key length: 150 characters, Maximum value length: 8192 characters.
	/// Is null by default.
	/// </remarks>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; }

	/// <summary>
	/// A read-only list of tags in a name-value format.
	/// </summary>
	/// <remarks>
	/// Is null by default.
	/// </remarks>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; }

	/// <summary>
	/// The UTC timestamp.
	/// </summary>
	public DateTime Time { get; }

	#endregion
}