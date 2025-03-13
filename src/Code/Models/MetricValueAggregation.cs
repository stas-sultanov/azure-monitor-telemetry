// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Models;

/// <summary>
/// Represents the aggregation of metric values within a sample set.
/// </summary>
public sealed class MetricValueAggregation
{
	/// <summary>
	/// The number of values in the sample set.
	/// </summary>
	public Int32 Count { get; init; }

	/// <summary>
	/// The max value of the metric across the sample set.
	/// </summary>
	public Double Max { get; init; }

	/// <summary>
	/// The min value of the metric across the sample set.
	/// </summary>
	public Double Min { get; init; }
}
