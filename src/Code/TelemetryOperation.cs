// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Represents an operation.
/// </summary>
/// <remarks>
/// This type is used to track and correlate telemetry data across different activities.
/// </remarks>
public sealed class TelemetryOperation
{
	#region Properties

	/// <summary>The identifier of the operation.</summary>
	public String? Id { get; init; }

	/// <summary>The name of the operation.</summary>
	public String? Name { get; init; }

	/// <summary>The identifier of the parent activity.</summary>
	public String? ParentId { get; init; }

	#endregion
}
