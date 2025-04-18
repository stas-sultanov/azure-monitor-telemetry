// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.Telemetry.Tests;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides equality comparison of two <see cref="KeyValuePair{TKey,TValue}" /> objects.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <remarks>
/// Initialize a new instance of <see cref="KeyValuePairEqualityComparer{TKey, TValue}" /> class.
/// </remarks>
/// <param name="keyComparer">An <see cref="IEqualityComparer{TKey}" /> to use to compare keys.</param>
/// <param name="valueComparer">An <see cref="IEqualityComparer{TValue}" /> to use to compare values.</param>
internal sealed class KeyValuePairEqualityComparer<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
	: IEqualityComparer<KeyValuePair<TKey, TValue>>
{
	#region Fields

	/// <summary>
	/// A key comparer.
	/// </summary>
	private readonly IEqualityComparer<TKey> keyComparer = keyComparer ?? throw new ArgumentNullException(nameof(keyComparer));

	/// <summary>
	/// A value comparer.
	/// </summary>
	private readonly IEqualityComparer<TValue> valueComparer = valueComparer ?? throw new ArgumentNullException(nameof(valueComparer));

	#endregion

	#region Methods of IEqualityComparer<KeyValuePair<TKey,TValue>>

	/// <summary>
	/// Determines whether the specified objects are equal.
	/// </summary>
	/// <param name="x">The first object of type to compare.</param>
	/// <param name="y">The second object of type to compare.</param>
	/// <returns><c>true</c> if the specified objects are equal; otherwise, <c>false</c>.</returns>
	public Boolean Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
	{
		return keyComparer.Equals(x.Key, y.Key) && valueComparer.Equals(x.Value, y.Value);
	}

	/// <summary>
	/// Returns a hash code for the specified object.
	/// </summary>
	/// <param name="obj">The <see cref="Object" /> for which a hash code is to be returned.</param>
	/// <returns>A hash code for the specified object.</returns>
	public Int32 GetHashCode(KeyValuePair<TKey, TValue> obj)
	{
		return obj.GetHashCode();
	}

	#endregion
}
