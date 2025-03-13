// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents information about an exception.
/// </summary>
public sealed class ExceptionInfo
{
	#region Properties

	/// <summary>
	/// A value indicating whether the full stack trace is available.
	/// </summary>
	public required Boolean HasFullStack { get; init; }

	/// <summary>
	/// The unique identifier for the exception.
	/// </summary>
	public required Int32 Id { get; init; }

	/// <summary>
	/// The message associated with the exception.
	/// </summary>
	public required String Message { get; init; }

	/// <summary>
	/// The identifier of the outer exception, if any.
	/// </summary>
	public required Int32 OuterId { get; init; }

	/// <summary>
	/// The stack information.
	/// </summary>
	public required IReadOnlyList<StackFrameInfo>? ParsedStack { get; init; }

	/// <summary>
	/// The type name of the exception.
	/// </summary>
	public required String TypeName { get; init; }

	#endregion
}
