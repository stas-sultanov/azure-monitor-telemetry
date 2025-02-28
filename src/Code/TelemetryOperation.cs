// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

using System.Runtime.CompilerServices;

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

	#region Methods

	/// <summary>
	/// Creates a new instance of <see cref="TelemetryOperation"/> with a new parent identifier.
	/// </summary>
	/// <param name="parentId">The new parent identifier to be set.</param>
	/// <returns>A new instance of <see cref="TelemetryOperation"/> with the specified parent identifier.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TelemetryOperation CloneWithNewParentId(String? parentId)
	{
		var result = new TelemetryOperation
		{
			Id = Id,
			Name = Name,
			ParentId = parentId
		};

		return result;
	}

	/// <summary>
	/// Creates a new instance of <see cref="TelemetryOperation"/> with a new parent identifier.
	/// </summary>
	/// <param name="parentId">The new parent identifier to be set.</param>
	/// <param name="previousParentId">Outputs the previous parent identifier.</param>
	/// <returns>A new instance of <see cref="TelemetryOperation"/> with the specified parent identifier.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TelemetryOperation CloneWithNewParentId
	(
		String? parentId,
		out String? previousParentId
	)
	{
		previousParentId = ParentId;

		var result = CloneWithNewParentId(parentId);

		return result;
	}

	#endregion
}
