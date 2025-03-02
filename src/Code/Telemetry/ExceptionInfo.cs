// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

using System;
using System.Collections.Generic;

public sealed class ExceptionInfo
{
	#region Properties

	public required Boolean HasFullStack { get; init; }

	public required Int32 Id { get; init; }

	public required String Message { get; init; }

	public required Int32 OuterId { get; init; }

	public required String TypeName { get; init; }

	public required IReadOnlyList<ExceptionFrameInfo> ParsedStack { get; init; }

	#endregion
}
