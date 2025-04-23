// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.Telemetry.Publish;

/// <summary>
/// Represents a Bearer access token with expiration date.
/// </summary>
public readonly struct BearerToken
{
	#region Properties

	/// <summary>
	/// Time when the token expires.
	/// </summary>
	public required DateTimeOffset ExpiresOn { get; init; }

	/// <summary>
	/// The token value.
	/// </summary>
	public required String Value { get; init; }

	#endregion
}
