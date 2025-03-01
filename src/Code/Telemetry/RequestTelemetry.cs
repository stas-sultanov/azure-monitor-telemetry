// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Represents telemetry of a logical sequence of execution triggered by an external request to an application.
/// </summary>
/// <remarks>
/// Every request execution is identified by a unique <see cref="Id"/> and <see cref="Url"/> that contain all the execution parameters.
/// </remarks>
public sealed class RequestTelemetry : ActivityTelemetry
{
	#region Properties

	/// <summary>
	/// The time taken to complete.
	/// </summary>
	public TimeSpan Duration { get; init; }

	/// <summary>
	/// The unique identifier.
	/// </summary>
	public required String Id { get; init; }

	/// <summary>
	/// A read-only list of measurements.
	/// </summary>
	/// <remarks>
	/// Maximum key length: 150 characters.
	/// Is null by default.
	/// </remarks>
	public IReadOnlyList<KeyValuePair<String, Double>>? Measurements { get; init; }

	/// <summary>
	/// The name of the request.
	/// </summary>
	public String? Name { get; init; }

	/// <inheritdoc/>
	public required TelemetryOperation Operation { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; init; }

	/// <summary>
	/// The result of an operation execution.
	/// It's the HTTP status code for HTTP requests.
	/// It might be an HRESULT value or an exception type for other request types.
	/// </summary>
	/// <remarks>Maximum length: 1024 characters.</remarks>
	public required String ResponseCode { get; init; }

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
