// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Models;

/// <summary>
/// Represents telemetry of a dependency call in an application.
/// </summary>
public sealed class DependencyTelemetry : ActivityTelemetry
{
	#region Properties

	/// <summary>
	/// The command initiated by this dependency call.
	/// </summary>
	/// <remarks>Maximum length: 8192 characters.</remarks>
	public String? Data { get; init; }

	/// <inheritdoc/>
	public TimeSpan Duration { get; init; }

	/// <inheritdoc/>
	public required String Id { get; init; }

	/// <summary>
	/// A read-only list of measurements.
	/// </summary>
	/// <remarks>Maximum key length: 150 characters.</remarks>
	public IReadOnlyList<KeyValuePair<String, Double>>? Measurements { get; init; }

	/// <summary>
	/// The name of the command initiated the dependency call.
	/// </summary>
	/// <remarks>Maximum length: 1024 characters.</remarks>
	public required String Name { get; init; }

	/// <summary>
	/// This field is the result code of a dependency call.
	/// </summary>
	/// <remarks>Maximum length: 1024 characters.</remarks>
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
	/// <remarks>Maximum length: 1024 characters.</remarks>
	public String? Target { get; init; }

	/// <summary>
	/// The UTC timestamp when the dependency call was initiated.
	/// </summary>
	public required DateTime Time { get; init; }

	/// <summary>
	/// The dependency type name.
	/// </summary>
	/// <remarks>Maximum length: 1024 characters.</remarks>
	public String? Type { get; init; }

	#endregion
}
