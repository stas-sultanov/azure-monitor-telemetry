// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Models;

/// <summary>
/// Represents telemetry of a logical sequence of execution triggered by an external request to an application.
/// </summary>
public sealed class RequestTelemetry : ActivityTelemetry
{
	#region Properties

	/// <summary>
	/// The time taken to complete.
	/// </summary>
	public TimeSpan Duration { get; init; }

	/// <inheritdoc/>
	public required String Id { get; init; }

	/// <summary>
	/// A read-only list of measurements.
	/// </summary>
	/// <remarks>Maximum key length: 150 characters.</remarks>
	public IReadOnlyList<KeyValuePair<String, Double>>? Measurements { get; init; }

	/// <summary>
	/// The name of the request.
	/// </summary>
	/// <remarks>Maximum length: 1024 characters.</remarks>
	public String? Name { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; init; }

	/// <summary>
	/// The result of an execution.
	/// </summary>
	/// <remarks>Maximum length: 1024 characters.</remarks>
	public required String ResponseCode { get; init; }

	/// <summary>
	/// The source of the request.
	/// </summary>
	/// <remarks>Maximum length: 1024 characters.</remarks>
	public String? Source { get; init; }

	/// <summary>
	/// A value indicating whether the operation was successful or unsuccessful.
	/// </summary>
	public Boolean Success { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; init; }

	/// <summary>
	/// The UTC timestamp when the request was initiated.
	/// </summary>
	public required DateTime Time { get; init; }

	/// <summary>
	/// The request URL.
	/// </summary>
	/// <remarks>Maximum length: 2048 characters.</remarks>
	public required Uri Url { get; init; }

	#endregion
}
