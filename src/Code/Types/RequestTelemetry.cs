// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Represents telemetry of a logical sequence of execution triggered by an external request to an application.
/// </summary>
/// <remarks>
/// Every request execution is identified by a unique <see cref="Id"/> and <see cref="Url"/> that contain all the execution parameters.
/// </remarks>
/// <param name="operation">The distributed operation context.</param>
/// <param name="time">The UTC timestamp when the request was initiated.</param>
/// <param name="id">The unique identifier.</param>
/// <param name="url">The request url.</param>
/// <param name="responseCode">The result of an operation execution.</param>
public sealed class RequestTelemetry
(
	OperationContext operation,
	DateTime time,
	String id,
	Uri url,
	String responseCode
)
	: Telemetry
{
	#region Properties

	/// <summary>
	/// The time taken to complete.
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
	/// The name of the request.
	/// </summary>
	public String? Name { get; init; }

	/// <inheritdoc/>
	public OperationContext Operation { get; } = operation;

	/// <inheritdoc/>
	public KeyValuePair<String, String>[]? Properties { get; init; }

	/// <summary>
	/// The result of an operation execution.
	/// It's the HTTP status code for HTTP requests.
	/// It might be an HRESULT value or an exception type for other request types.
	/// </summary>
	/// <remarks>Maximum length: 1024 characters.</remarks>
	public String ResponseCode { get; } = responseCode;

	/// <summary>
	/// A value indicating whether the operation was successful or unsuccessful.
	/// </summary>
	public Boolean Success { get; init; }

	/// <inheritdoc/>
	public KeyValuePair<String, String>[]? Tags { get; init; }

	/// <summary>
	/// The UTC timestamp when the request was initiated.
	/// </summary>
	public DateTime Time { get; } = time;

	/// <summary>
	/// The request URL.
	/// </summary>
	/// <remarks>Maximum length: 2048 characters.</remarks>
	public Uri Url { get; } = url;

	#endregion
}
