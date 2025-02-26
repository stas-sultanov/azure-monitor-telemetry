// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Represents telemetry of a dependency call in an application.
/// </summary>
/// <remarks>
/// Examples of dependencies include:
/// - SQL queries
/// - HTTP calls to external services
/// - Azure storage operations
/// - Custom dependencies
/// </remarks>
/// <param name="operation">The distributed operation context.</param>
/// <param name="time">The UTC timestamp when the dependency call was initiated.</param>
/// <param name="id">The unique identifier.</param>
/// <param name="name">The name of the command initiated the dependency call.</param>
public sealed class DependencyTelemetry
(
	TelemetryOperation operation,
	DateTime time,
	String id,
	String name
)
	: Telemetry
{
	#region Properties

	/// <summary>
	/// This field is the command initiated by this dependency call.
	/// </summary>
	/// <example>SQL statement and HTTP URL with all query parameters.</example>
	public String? Data { get; init; }

	/// <summary>
	/// The time taken to complete the dependency call.
	/// </summary>
	public TimeSpan Duration { get; init; }

	/// <summary>
	/// The unique identifier.
	/// </summary>
	public String Id { get; } = id;

	/// <summary>
	/// A read-only list of measurements.
	/// </summary>
	/// <remarks>
	/// Maximum key length: 150 characters.
	/// Is null by default.
	/// </remarks>
	public IReadOnlyList<KeyValuePair<String, Double>>? Measurements { get; init; }

	/// <summary>
	/// The name of the command initiated the dependency call.
	/// </summary>
	public String Name { get; } = name;

	/// <inheritdoc/>
	public TelemetryOperation Operation { get; } = operation;

	/// <summary>
	/// This field is the result code of a dependency call.
	/// </summary>
	/// <example>SQL error code, HTTP status code.</example>
	public String? ResultCode { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; init; }

	/// <summary>
	/// A value indicating whether the operation was successful or unsuccessful.
	/// </summary>
	public Boolean Success { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; init; }

	/// <summary>
	/// This field is the target site of a dependency call.
	/// </summary>
	/// <example>Server name, host address.</example>
	public String? Target { get; init; }

	/// <summary>
	/// The UTC timestamp when the dependency call was initiated.
	/// </summary>
	public DateTime Time { get; } = time;

	/// <summary>
	/// This field is the dependency type name.
	/// It has a low cardinality value for logical grouping of dependencies and interpretation of other fields like commandName and resultCode.
	/// </summary>
	/// <example>SQL, Azure table, HTTP.</example>
	public String? Type { get; init; }

	#endregion
}
