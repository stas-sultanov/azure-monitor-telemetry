// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Types;

using Azure.Monitor.Telemetry;

/// <summary>
/// Represents telemetry of an exception that occurred in an application.
/// </summary>
/// <remarks>
/// This class is used to track and report exceptions in the application, including their stack traces
/// and other relevant details. The maximum length of the stack trace is limited to 32768 characters.
/// </remarks>
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
	/// <remarks>
	/// Maximum key length: 150 characters.
	/// Is null by default.
	/// </remarks>
	public IReadOnlyList<KeyValuePair<String, Double>>? Measurements { get; init; }

	/// <inheritdoc/>
	public required TelemetryOperation Operation { get; init; }

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
