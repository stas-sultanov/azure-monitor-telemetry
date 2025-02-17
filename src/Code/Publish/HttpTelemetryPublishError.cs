// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Publish;

using System.Net;

/// <summary>
/// Represents an HTTP track response error with an index, message, and status code.
/// </summary>
/// <param name="index">The zero-based index of the error in a sequence.</param>
/// <param name="message">The error message describing what went wrong.</param>
/// <param name="statusCode">The HTTP status code associated with the error.</param>
public sealed class HttpTelemetryPublishError
(
	UInt16 index,
	String message,
	HttpStatusCode statusCode
)
{
	#region Data

	/// <summary>
	/// The zero-based index of the error in a sequence.
	/// </summary>
	public UInt16 Index { get; } = index;

	/// <summary>
	/// The error message describing what went wrong.
	/// </summary>
	public String Message { get; } = message;

	/// <summary>
	/// The HTTP status code associated with the error.
	/// </summary>
	public HttpStatusCode StatusCode { get; } = statusCode;

	#endregion
}
