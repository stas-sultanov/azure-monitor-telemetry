// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.Telemetry;

/// <summary>
/// The enumeration defines severity levels used by the service.
/// </summary>
public enum SeverityLevel : Int32
{
	/// <summary>
	/// Verbose severity level.
	/// </summary>
	Verbose = 0,

	/// <summary>
	/// Information severity level.
	/// </summary>
	Information = 1,

	/// <summary>
	/// Warning severity level.
	/// </summary>
	Warning = 2,

	/// <summary>
	/// Error severity level.
	/// </summary>
	Error = 3,

	/// <summary>
	/// Critical severity level.
	/// </summary>
	Critical = 4
}
