// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.Telemetry.Models;

/// <summary>
/// Represents telemetry of an availability test.
/// </summary>
public sealed class AvailabilityTelemetry : ActivityTelemetry
{
	#region Properties

	/// <inheritdoc/>
	public required TimeSpan Duration { get; init; }

	/// <inheritdoc/>
	public required String Id { get; init; }

	/// <summary>
	/// A read-only list of measurements.
	/// </summary>
	/// <remarks>Maximum key length: 150 characters.</remarks>
	public IReadOnlyList<KeyValuePair<String, Double>>? Measurements { get; init; }

	/// <summary>
	/// The message.
	/// </summary>
	/// <remarks>Maximum length: 8192 characters.</remarks>
	public required String Message { get; init; }

	/// <summary>
	/// The name.
	/// </summary>
	/// <remarks>Maximum length: 1024 characters.</remarks>
	public required String Name { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; init; }

	/// <summary>
	/// Location from where the test has been performed.
	/// </summary>
	/// <remarks>Maximum length: 2048 characters.</remarks>
	public String? RunLocation { get; init; }

	/// <summary>
	/// A value indicating whether the operation was successful or unsuccessful.
	/// </summary>
	public required Boolean Success { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; init; }

	/// <summary>
	/// The UTC timestamp when the test was initiated.
	/// </summary>
	public required DateTime Time { get; init; }

	#endregion
}
