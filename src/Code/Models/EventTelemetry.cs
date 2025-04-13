// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Models;

/// <summary>
/// Represents telemetry of an event that occurred in an application.
/// </summary>
/// <remarks>
/// Typically, it is a user interaction such as a button click or an order checkout.
/// It can also be an application lifecycle event like initialization or a configuration update.
/// Semantically, events might or might not be correlated to requests.
/// If used properly, event telemetry is more important than requests or traces.
/// </remarks>
public sealed class EventTelemetry : Telemetry
{
	#region Properties

	/// <summary>
	/// A read-only list of measurements.
	/// </summary>
	/// <remarks>Maximum key length: 150 characters.</remarks>
	public IReadOnlyList<KeyValuePair<String, Double>>? Measurements { get; init; }

	/// <summary>
	/// The name.
	/// </summary>
	/// <remarks>Maximum length: 512 characters.</remarks>
	public required String Name { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; init; }

	/// <summary>
	/// The UTC timestamp when the event has occurred.
	/// </summary>
	public required DateTime Time { get; init; }

	#endregion
}
