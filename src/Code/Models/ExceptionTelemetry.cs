// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Models;

using Azure.Monitor.Telemetry;

/// <summary>
/// Represents telemetry of an exception that occurred in an application.
/// </summary>
public sealed class ExceptionTelemetry : Telemetry
{
	#region Properties

	/// <summary>
	/// A read only list that represents exceptions stack.
	/// </summary>
	public required IReadOnlyList<ExceptionInfo> Exceptions { get; init; }

	/// <summary>
	/// A read-only list of measurements.
	/// </summary>
	/// <remarks>Maximum key length: 150 characters.</remarks>
	public IReadOnlyList<KeyValuePair<String, Double>>? Measurements { get; init; }

	/// <summary>
	/// The problem identifier.
	/// </summary>
	/// <remarks>Maximum length: 1024 characters.</remarks>
	public String? ProblemId { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; init; }

	/// <summary>
	/// The severity level.
	/// </summary>
	public SeverityLevel? SeverityLevel { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; init; }

	/// <summary>
	/// The UTC timestamp when the exception has occurred.
	/// </summary>
	public required DateTime Time { get; init; }

	#endregion
}
