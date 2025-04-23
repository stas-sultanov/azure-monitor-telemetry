// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.Telemetry.Models;

using System;

/// <summary>
/// Represents information about a stack frame in a call stack.
/// </summary>
public sealed class StackFrameInfo
{
	#region Properties

	/// <summary>
	/// The name of the assembly where the stack frame is located.
	/// </summary>
	public required String Assembly { get; init; }

	/// <summary>
	/// The file name that contains the code that was executed.
	/// </summary>
	public required String? FileName { get; init; }

	/// <summary>
	/// The level of the stack frame in the call stack.
	/// </summary>
	public required Int32 Level { get; init; }

	/// <summary>
	/// The line number in the source code where the stack frame is located.
	/// </summary>
	public required Int32 Line { get; init; }

	/// <summary>
	/// The name of the method where the stack frame is located.
	/// </summary>
	public required String? Method { get; init; }

	#endregion
}
