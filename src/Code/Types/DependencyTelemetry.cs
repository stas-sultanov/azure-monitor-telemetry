// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Types;

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
public sealed class DependencyTelemetry : ActivityTelemetry
{
	#region Properties

	/// <summary>
	/// The command initiated by this dependency call.
	/// </summary>
	/// <example>SQL statement or HTTP URL with all query parameters.</example>
	public String? Data { get; init; }

	/// <inheritdoc/>
	public TimeSpan Duration { get; init; }

	/// <inheritdoc/>
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
	/// The name of the command initiated the dependency call.
	/// </summary>
	public required String Name { get; init; }

	/// <inheritdoc/>
	public required TelemetryOperation Operation { get; init; }

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
	public required DateTime Time { get; init; }

	/// <summary>
	/// This field is the dependency type name.
	/// It has a low cardinality value for logical grouping of dependencies and interpretation of other fields like commandName and resultCode.
	/// </summary>
	/// <example>SQL, Azure table, HTTP.</example>
	public String? Type { get; init; }

	#endregion
}
