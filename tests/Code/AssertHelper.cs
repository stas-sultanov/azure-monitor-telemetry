// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.Telemetry.Tests;

using System;
using System.Linq.Expressions;

using Azure.Monitor.Telemetry.Models;

/// <summary>
/// Provides helper methods for asserting.
/// </summary>
internal static class AssertHelper
{
	#region Methods: public

	/// <summary>
	/// Tests if the value of the property of the <paramref name="actualObject"/> object equals to <paramref name="expectedValue"/>.
	/// </summary>
	/// <typeparam name="ObjectType">The type of the object.</typeparam>
	/// <typeparam name="ValueType">The type of the value.</typeparam>
	/// <param name="actualObject">The object.</param>
	/// <param name="propertySelector">An expression to get property from the <paramref name="actualObject"/>.</param>
	/// <param name="expectedValue">The expected value.</param>
	/// <exception cref="AssertFailedException">Thrown if <paramref name="expectedValue"/> is not equal to property value.</exception>
	public static void PropertyEqualsTo<ObjectType, ValueType>
	(
		ObjectType actualObject,
		Expression<Func<ObjectType, ValueType>> propertySelector,
		ValueType expectedValue
	)
	{
		GetPropertyValueAndMetadata(actualObject, propertySelector, out var actualValue, out var typeName, out var propertyName);

		if (EqualityComparer<ValueType>.Default.Equals(expectedValue, actualValue))
		{
			return;
		}

		throw new AssertFailedException($"{typeName}.{propertyName} expected: {expectedValue} actual: {actualValue}");
	}

	/// <summary>
	/// Tests if the value of the property of the <paramref name="actualObject"/> object equals to <paramref name="expectedValue"/>.
	/// </summary>
	/// <typeparam name="ObjectType">The type of the object.</typeparam>
	/// <typeparam name="ItemType">The type of the key within the enumeration.</typeparam>
	/// <param name="actualObject">The object.</param>
	/// <param name="propertySelector">An expression to get property from the <paramref name="actualObject"/>.</param>
	/// <param name="expectedValue">The expected value.</param>
	/// <exception cref="AssertFailedException">Thrown if <paramref name="expectedValue"/> is not equal to property value.</exception>
	public static void PropertyEqualsTo<ObjectType, ItemType>
	(
		ObjectType actualObject,
		Expression<Func<ObjectType, IEnumerable<ItemType>?>> propertySelector,
		IEnumerable<ItemType>? expectedValue,
		IEqualityComparer<ItemType>? itemComparer
	)
	{
		GetPropertyValueAndMetadata(actualObject, propertySelector, out var actualValue, out var typeName, out var propertyName);

		if (ReferenceEquals(expectedValue, actualValue))
		{
			return;
		}

		if (expectedValue == null || actualValue == null)
		{
			throw new AssertFailedException($"{typeName}.{propertyName} expected:{expectedValue} actual:{actualValue}");
		}

		var expectedCount = expectedValue.Count();
		var actualCount = actualValue.Count();

		if (expectedCount != actualCount)
		{
			throw new AssertFailedException($"{typeName}.{propertyName}.Count expected:{expectedCount} actual:{actualCount}");
		}

		var comparer = itemComparer ?? EqualityComparer<ItemType>.Default;

		foreach (var item in expectedValue)
		{
			if (!actualValue.Contains(item, comparer))
			{
				throw new AssertFailedException($"{typeName}.{propertyName} does not contain item: {item}");
			}
		}
	}

	/// <summary>
	/// Tests if the value of the property of the <paramref name="actualObject"/> object equals to <paramref name="expectedValue"/>.
	/// </summary>
	/// <typeparam name="ObjectType">The type of the object.</typeparam>
	/// <typeparam name="KeyType">The type of the key within the <see cref="KeyValuePair{TKey, TValue}"/>.</typeparam>
	/// <typeparam name="ValueType">The type of the value within the <see cref="KeyValuePair{TKey, TValue}"/>.</typeparam>
	/// <param name="actualObject">The object.</param>
	/// <param name="propertySelector">An expression to get property from the <paramref name="actualObject"/>.</param>
	/// <param name="expectedValue">The expected value.</param>
	/// <exception cref="AssertFailedException">Thrown if <paramref name="expectedValue"/> is not equal to property value.</exception>
	public static void PropertyEqualsTo<ObjectType, KeyType, ValueType>
	(
		ObjectType actualObject,
		Expression<Func<ObjectType, IEnumerable<KeyValuePair<KeyType, ValueType>>?>> propertySelector,
		IEnumerable<KeyValuePair<KeyType, ValueType>>? expectedValue
	)
	{
		var comparer = new KeyValuePairEqualityComparer<KeyType, ValueType>
		(
			EqualityComparer<KeyType>.Default,
			EqualityComparer<ValueType>.Default
		);

		PropertyEqualsTo(actualObject, propertySelector, expectedValue, comparer);
	}

	/// <summary>
	/// Tests if the value of the property of the <paramref name="actualObject"/> object equals to <paramref name="expectedValue"/>.
	/// </summary>
	/// <typeparam name="ObjectType">The type of the object.</typeparam>
	/// <param name="actualObject">The object.</param>
	/// <param name="propertySelector">An expression to get property from the <paramref name="actualObject"/>.</param>
	/// <param name="expectedValue">The expected value.</param>
	/// <exception cref="AssertFailedException">Thrown if <paramref name="expectedValue"/> is not equal to property value.</exception>
	public static void PropertyEqualsTo<ObjectType>
	(
		ObjectType actualObject,
		Expression<Func<ObjectType, MetricValueAggregation?>> propertySelector,
		MetricValueAggregation expectedValue
	)
	{
		GetPropertyValueAndMetadata(actualObject, propertySelector, out var actualValue, out var typeName, out var propertyName);

		if (ReferenceEquals(expectedValue, actualValue))
		{
			return;
		}

		if (actualValue == null)
		{
			throw new AssertFailedException
			(
				$"{typeName}.{propertyName}"
			);
		}

		PropertyEqualsTo(actualValue, o => o.Count, expectedValue.Count);

		PropertyEqualsTo(actualValue, o => o.Max, expectedValue.Max);

		PropertyEqualsTo(actualValue, o => o.Min, expectedValue.Min);
	}

	/// <summary>
	/// Tests if the <paramref name="evaluate"/> returns <c>true</c> and throws an exception if <c>false</c> is returned.
	/// </summary>
	/// <typeparam name="ObjectType">The type of the object.</typeparam>
	/// <typeparam name="ValueType">The type of the value.</typeparam>
	/// <param name="actualObject">The object.</param>
	/// <param name="propertySelector">An expression to get property from the <paramref name="actualObject"/>.</param>
	/// <param name="evaluate">The evaluation code.</param>
	/// <exception cref="AssertFailedException">Thrown if <paramref name="evaluate"/> returns <c>false</c>.</exception>
	public static void PropertyEvaluatesToTrue<ObjectType, ValueType>
	(
		ObjectType actualObject,
		Expression<Func<ObjectType, ValueType>> propertySelector,
		Func<ValueType, Boolean> evaluate
	)
	{
		GetPropertyValueAndMetadata(actualObject, propertySelector, out var actual, out var typeName, out var propertyName);

		var testResult = evaluate(actual);

		if (testResult)
		{
			return;
		}

		throw new AssertFailedException
		(
			$"{typeName}.{propertyName}"
		);
	}

	#endregion

	#region Methods: private

	public static void GetPropertyValueAndMetadata<ObjectType, PropertyType>
	(
		ObjectType actualObject,
		Expression<Func<ObjectType, PropertyType>> propertySelector,
		out PropertyType propertyValue,
		out String typeName,
		out String propertyName
	)
	{
		typeName = typeof(ObjectType).Name;

		propertyName = ((MemberExpression) propertySelector.Body).Member.Name;

		propertyValue = propertySelector.Compile()(actualObject);
	}

	#endregion
}
