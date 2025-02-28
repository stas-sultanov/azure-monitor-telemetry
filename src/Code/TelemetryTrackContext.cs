// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Code;

using System;

public sealed class OperationStartInfo
{
	public required String Id { get; init; }

	public required TelemetryOperation Operation { get; init; }

	public required DateTime Time { get; init; }

	public required Int64 Timestamp { get; init; }
}