// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Contract for types that reperests telemetry.
/// </summary>
public interface Telemetry
{
	#region Properties

	/// <summary>
	/// The details about the operation being performed.
	/// </summary>
	public TelemetryOperation Operation { get; }

	/// <summary>
	/// A read-only list of custom properties.
	/// </summary>
	/// <remarks>
	/// This collection is used to extend standard telemetry data with custom dimensions.
	/// Maximum key length: 150 characters, Maximum value length: 8192 characters.
	/// </remarks>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; }

	/// <summary>
	/// A read-only list of tags.
	/// </summary>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; }

	/// <summary>
	/// The UTC timestamp.
	/// </summary>
	public DateTime Time { get; }

	#endregion
}