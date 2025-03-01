// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Represents a printf-style trace statement that is text searched.
/// </summary>
public sealed class TraceTelemetry : Telemetry
{
	#region Properties

	/// <summary>
	/// The message.
	/// </summary>
	/// <remarks>Maximum length: 32768 characters.</remarks>
	public required String Message { get; init; }

	/// <inheritdoc/>
	public required TelemetryOperation Operation { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; init; }

	/// <summary>
	/// The severity level.
	/// </summary>
	public SeverityLevel SeverityLevel { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; init; }

	/// <summary>
	/// The UTC timestamp when the trace has occurred.
	/// </summary>
	public required DateTime Time { get; init; }

	#endregion
}
