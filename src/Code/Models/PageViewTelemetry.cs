// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Models;

/// <summary>
/// Represents telemetry of a page view.
/// </summary>
/// <remarks>
/// The page is a logical unit that is defined by the developer to be an application tab or a screen.
/// </remarks>
public sealed class PageViewTelemetry : ActivityTelemetry
{
	#region Properties

	/// <inheritdoc/>
	public TimeSpan Duration { get; init; }

	/// <inheritdoc/>
	public required String Id { get; init; }

	/// <summary>
	/// A read-only list of measurements.
	/// </summary>
	/// <remarks>
	/// Maximum key length: 150 characters.
	/// Is null by default.
	/// </remarks>
	public IReadOnlyList<KeyValuePair<String, Double>>? Measurements { get; init; }

	/// <summary>
	/// The name of the page.
	/// </summary>
	public required String Name { get; init; }

	/// <inheritdoc/>
	public required TelemetryOperation Operation { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; init; }

	/// <summary>
	/// The UTC timestamp when the page view was initiated.
	/// </summary>
	public required DateTime Time { get; init; }

	/// <summary>
	/// The request URL.
	/// </summary>
	/// <remarks>Maximum length: 2048 characters.</remarks>
	public Uri? Url { get; init; }

	#endregion
}
