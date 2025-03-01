// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;
/// <summary>
/// Represents a distributed operation containing information about operation hierarchy and synthetic sources.
/// </summary>
/// <remarks>
/// This type is used to track and correlate telemetry data across different operations and their relationships.
/// </remarks>
public sealed class TelemetryOperation
{
	#region Static

	/// <summary>
	/// An empty instance of <see cref="TelemetryOperation"/>.
	/// </summary>
	public static TelemetryOperation Empty { get; } = new TelemetryOperation();

	#endregion

	#region Properties

	/// <summary>The identifier of the topmost operation.</summary>
	public String? Id { get; init; }

	/// <summary>The name of the topmost operation.</summary>
	public String? Name { get; init; }

	/// <summary>The identifier of the parent operation.</summary>
	public String? ParentId { get; init; }

	#endregion
}
