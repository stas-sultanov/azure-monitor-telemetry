// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.Telemetry;

/// <summary>
/// A contract for types that represents telemetry.
/// </summary>
public interface Telemetry
{
	#region Properties

	/// <summary>
	/// A read-only list of custom properties.
	/// </summary>
	/// <remarks>Maximum key length: 150 characters, Maximum value length: 8192 characters.</remarks>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; }

	/// <summary>
	/// A read-only list of tags.
	/// </summary>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; }

	/// <summary>
	/// The UTC timestamp.
	/// </summary>
	public DateTime Time { get; }

	#endregion
}
