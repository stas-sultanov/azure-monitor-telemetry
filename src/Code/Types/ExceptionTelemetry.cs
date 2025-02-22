// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;
/// <summary>
/// Represents telemetry of an exception that occurred in an application.
/// </summary>
/// <remarks>
/// This class is used to track and report exceptions in the application, including their stack traces
/// and other relevant details. The maximum length of the stack trace is limited to 32768 characters.
/// </remarks>
/// <param name="time">The UTC timestamp when the exception has occurred.</param>
/// <param name="exception">The exception object.</param>
public sealed class ExceptionTelemetry
(
	DateTime time,
	Exception exception
)
	: Telemetry
{
	#region Properties

	/// <summary>
	/// The exception object.
	/// </summary>
	public Exception Exception { get; } = exception;

	/// <summary>
	/// A collection of measurements.
	/// </summary>
	/// <remarks>
	/// Maximum key length: 150 characters.
	/// Is null by default.
	/// </remarks>
	public KeyValuePair<String, Double>[] Measurements { get; init; }

	/// <inheritdoc/>
	public OperationContext Operation { get; init; }

	/// <inheritdoc/>
	public KeyValuePair<String, String>[] Properties { get; init; }

	/// <summary>
	/// The severity level.
	/// </summary>
	public SeverityLevel? SeverityLevel { get; init; }

	/// <inheritdoc/>
	public KeyValuePair<String, String>[] Tags { get; init; }

	/// <summary>
	/// The UTC timestamp when the exception has occurred.
	/// </summary>
	public DateTime Time { get; } = time;

	#endregion
}
