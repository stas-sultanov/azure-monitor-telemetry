// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Represents telemetry of an availability test.
/// </summary>
/// <param name="operation">The distributed operation context.</param>
/// <param name="time">The UTC timestamp when the test was initiated.</param>
/// <param name="id">The unique identifier.</param>
/// <param name="name">The name of the telemetry instance.</param>
/// <param name="message">The message associated with the telemetry instance.</param>
public sealed class AvailabilityTelemetry
(
	TelemetryOperation operation,
	DateTime time,
	String id,
	String name,
	String message
)
	: Telemetry
{
	#region Properties

	/// <summary>
	/// The time taken to complete the test.
	/// </summary>
	public TimeSpan Duration { get; init; }

	/// <summary>
	/// The unique identifier.
	/// </summary>
	public String Id { get; } = id;

	/// <summary>
	/// A collection of measurements.
	/// </summary>
	/// <remarks>
	/// Maximum key length: 150 characters.
	/// Is null by default.
	/// </remarks>
	public KeyValuePair<String, Double>[]? Measurements { get; init; }

	/// <summary>
	/// The message.
	/// </summary>
	public String Message { get; } = message;

	/// <summary>
	/// The name.
	/// </summary>
	public String Name { get; } = name;

	/// <inheritdoc/>
	public TelemetryOperation Operation { get; } = operation;

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; init; }

	/// <summary>
	/// Location from where the test has been performed.
	/// </summary>
	public String? RunLocation { get; init; }

	/// <summary>
	/// A value indicating whether the operation was successful or unsuccessful.
	/// </summary>
	public Boolean Success { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; init; }

	/// <summary>
	/// The UTC timestamp when the test was initiated.
	/// </summary>
	public DateTime Time { get; } = time;

	#endregion
}