// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Code;

using System;

public sealed class OperationInfo
{
	/// <summary>
	/// The time taken to complete.
	/// </summary>
	public TimeSpan Duration { get; }

	/// <summary>
	/// The unique identifier.
	/// </summary>
	public required String Id { get; init; }

	/// <summary>
	/// The UTC timestamp when the trace has occurred.
	/// </summary>
	public DateTime Time { get; }
}
