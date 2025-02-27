// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Publish;

using System.Net;

/// <summary>
/// Represents an HTTP track response error with an index, message, and status code.
/// </summary>
public sealed class HttpTelemetryPublishError
{
	#region Data

	/// <summary>
	/// The zero-based index of the error in a sequence.
	/// </summary>
	public required UInt16 Index { get; init; }

	/// <summary>
	/// The error message describing what went wrong.
	/// </summary>
	public required String Message { get; init; }

	/// <summary>
	/// The HTTP status code associated with the error.
	/// </summary>
	public required HttpStatusCode StatusCode { get; init; }

	#endregion
}
