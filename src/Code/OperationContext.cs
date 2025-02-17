// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Represents a distributed operation containing information about operation hierarchy and synthetic sources.
/// </summary>
/// <remarks>
/// This type is used to track and correlate telemetry data across different operations and their relationships.
/// </remarks>
public sealed record OperationContext
{
	#region Properties

	/// <summary>The topmost operation identifier.</summary>
	public String Id { get; init; }

	/// <summary>The topmost operation name.</summary>
	public String Name { get; init; }

	/// <summary>The parent operation identifier.</summary>
	public String ParentId { get; init; }

	/// <summary>The synthetic source.</summary>
	public String SyntheticSource { get; init; }

	#endregion
}
