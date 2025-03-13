// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Models;

/// <summary>
/// Represents telemetry of an aggregated metric data.
/// </summary>
public sealed class MetricTelemetry : Telemetry
{
	#region Properties

	/// <summary>
	/// The name.
	/// </summary>
	/// <remarks>Maximum length: 512 characters.</remarks>
	public required String Name { get; init; }

	/// <summary>
	/// The namespace.
	/// </summary>
	public required String Namespace { get; init; }

	/// <inheritdoc/>
	public required TelemetryOperation Operation { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; init; }

	/// <summary>
	/// The UTC timestamp when the metric was recorded.
	/// </summary>
	public required DateTime Time { get; init; }

	/// <summary>
	/// The value.
	/// </summary>
	public required Double Value { get; init; }

	/// <summary>
	/// The aggregation of metric values within a sample set.
	/// </summary>
	public MetricValueAggregation? ValueAggregation { get; init; }

	#endregion
}
