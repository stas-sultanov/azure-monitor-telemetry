// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Publish;

/// <summary>
/// Represents a Bearer access token.
/// </summary>
/// <param name="value">The token value.</param>
/// <param name="expiresOn">Time when the token expires.</param>
public readonly struct BearerToken
(
	String value,
	DateTimeOffset expiresOn
)
{
	#region Properties

	/// <summary>
	/// Time when the token expires.
	/// </summary>
	public DateTimeOffset ExpiresOn { get; } = expiresOn;

	/// <summary>
	/// The token value.
	/// </summary>
	public String Value { get; } = value;

	#endregion
}
