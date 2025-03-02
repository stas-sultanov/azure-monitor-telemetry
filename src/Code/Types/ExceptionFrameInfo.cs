// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Types;

using System;

public sealed class ExceptionFrameInfo
{
	#region Properties

	public required String Assembly { get; init; }

	public required Int32 Level { get; init; }

	public required Int32 Line { get; init; }

	public required String? Method { get; init; }

	#endregion
}