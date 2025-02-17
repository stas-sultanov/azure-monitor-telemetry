// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Represents telemetry of a page view.
/// </summary>
/// <remarks>
/// The page is a logical unit that is defined by the developer to be an application tab or a screen.
/// </remarks>
/// <param name="time">The UTC timestamp when the operation was initiated.</param>
/// <param name="id">The unique identifier.</param>
/// <param name="name">The name of the page.</param>
public sealed class PageViewTelemetry
(
	DateTime time,
	String id,
	String name
)
	: Telemetry
{
	#region Properties

	/// <summary>
	/// The time taken to present the page to the user.
	/// </summary>
	public TimeSpan Duration { get; init; }

	/// <summary>
	/// The unique identifier.
	/// </summary>
	public String Id { get; } = id;

	/// <summary>
	/// A collection of measurements.
	/// </summary>
	/// <remarks>
	/// Maximum key length: 150 characters.
	/// Is null by default.
	/// </remarks>
	public MeasurementList Measurements { get; init; }

	/// <summary>
	/// The name of the page.
	/// </summary>
	public String Name { get; } = name;

	/// <inheritdoc/>
	public OperationContext Operation { get; init; }

	/// <inheritdoc/>
	public PropertyList Properties { get; init; }

	/// <inheritdoc/>
	public TagList Tags { get; init; }

	/// <summary>
	/// The UTC timestamp when the operation was initiated.
	/// </summary>
	public DateTime Time { get; } = time;

	/// <summary>
	/// The request URL.
	/// </summary>
	/// <remarks>Maximum length: 2048 characters.</remarks>
	public Uri Url { get; init; }

	#endregion
}
