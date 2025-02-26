// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Represents telemetry of an aggregated metric data.
/// </summary>
/// <param name="operation">The distributed operation context.</param>
/// <param name="time">The UTC timestamp when the trace has occurred.</param>
/// <param name="namespace">The namespace.</param>
/// <param name="name">The name.</param>
/// <param name="value">The value.</param>
public sealed class MetricTelemetry
(
	TelemetryOperation operation,
	DateTime time,
	String @namespace,
	String name,
	Double value,
	MetricValueAggregation? valueAggregation = null
)
	: Telemetry
{
	#region Properties

	/// <summary>
	/// The name.
	/// </summary>
	/// <remarks>Maximum length: 512 characters.</remarks>
	public String Name { get; } = name;

	/// <summary>
	/// The namespace.
	/// </summary>
	public String Namespace { get; } = @namespace;

	/// <inheritdoc/>
	public TelemetryOperation Operation { get; } = operation;

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; init; }

	/// <summary>
	/// The UTC timestamp when the metric was recorded.
	/// </summary>
	public DateTime Time { get; } = time;

	/// <summary>
	/// The value.
	/// </summary>
	public Double Value { get; } = value;

	public MetricValueAggregation? ValueAggregation { get; } = valueAggregation;

	#endregion
}
