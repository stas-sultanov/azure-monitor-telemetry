// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Represents a distributed operation containing information about operation hierarchy and synthetic sources.
/// </summary>
/// <remarks>
/// This type is used to track and correlate telemetry data across different operations and their relationships.
/// </remarks>
/// <param name="id">The identifier of the topmost operation.</param>
/// <param name="name">The name of the topmost operation.</param>
/// <param name="parentId">The identifier of the parent operation.</param>
/// <param name="syntheticSource">The synthetic source.</param>
public sealed class OperationContext
(
	String? id = null,
	String? name = null,
	String? parentId = null,
	String? syntheticSource = null
)
{
	#region Properties

	/// <summary>The identifier of the topmost operation.</summary>
	public String? Id { get; } = id;

	/// <summary>The name of the topmost operation.</summary>
	public String? Name { get; } = name;

	/// <summary>The identifier of the parent operation.</summary>
	public String? ParentId { get; } = parentId;

	/// <summary>The synthetic source.</summary>
	public String? SyntheticSource { get; } = syntheticSource;

	#endregion

	#region Methods

	/// <summary>
	/// Creates a new instance of <see cref="OperationContext"/> with a new parent identifier.
	/// </summary>
	/// <param name="parentId">The new parent identifier to be set.</param>
	/// <returns>A new instance of <see cref="OperationContext"/> with the specified parent identifier.</returns>
	public OperationContext CloneWithNewParentId(String? parentId)
	{
		return new OperationContext(Id, Name, parentId, SyntheticSource);
	}

	/// <summary>
	/// Creates a new instance of <see cref="OperationContext"/> with a new parent identifier.
	/// </summary>
	/// <param name="parentId">The new parent identifier to be set.</param>
	/// <param name="previousParentId">Outputs the previous parent identifier.</param>
	/// <returns>A new instance of <see cref="OperationContext"/> with the specified parent identifier.</returns>
	public OperationContext CloneWithNewParentId
	(
		String? parentId,
		out String? previousParentId
	)
	{
		var result = new OperationContext(Id, Name, parentId, SyntheticSource);

		previousParentId = ParentId;

		return result;
	}

	#endregion
}
