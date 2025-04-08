// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Tests;

using System;
using System.Linq.Expressions;

using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Models;

/// <summary>
/// Provides helper methods for asserting.
/// </summary>
internal static class AssertHelper
{
	#region Methods: public

	/// <summary>
	/// Tests whether the specified values are equal and throws an exception if the two values are not equal.
	/// </summary>
	/// <param name="expected">The first value to compare. This is the value the tests expects.</param>
	/// <param name="actual">The second value to compare. This is the value produced by the code under test.</param>
	/// <exception cref="AssertFailedException">Thrown if <paramref name="expected"/> is not equal to <paramref name="actual"/>.</exception>
	public static void AreEqual
	(
		TelemetryOperation expected,
		TelemetryOperation actual
	)
	{
		// check if both params are referencing to the same object
		if (ReferenceEquals(expected, actual))
		{
			return;
		}

		Assert.AreEqual(expected.Id, actual.Id, "{0}.{1}", nameof(TelemetryOperation), nameof(TelemetryOperation.Id));

		Assert.AreEqual(expected.Name, actual.Name, "{0}.{1}", nameof(TelemetryOperation), nameof(TelemetryOperation.Name));

		Assert.AreEqual(expected.ParentId, actual.ParentId, "{0}.{1}", nameof(TelemetryOperation), nameof(TelemetryOperation.ParentId));
	}

	/// <summary>
	/// Tests if the value of the property of the <paramref name="actualObject"/> object equals to <paramref name="expectedValue"/>.
	/// </summary>
	/// <typeparam name="ObjectType">The type of the object.</typeparam>
	/// <typeparam name="ValueType">The type of the value.</typeparam>
	/// <param name="actualObject">The object.</param>
	/// <param name="propertySelector">An expression to get property from the <paramref name="actualObject"/>.</param>
	/// <param name="expectedValue">The expected value.</param>
	/// <exception cref="AssertFailedException">Thrown if <paramref name="expectedValue"/> is not equal to property value.</exception>
	public static void AreEqual<ObjectType, ValueType>
	(
		ObjectType actualObject,
		Expression<Func<ObjectType, ValueType>> propertySelector,
		ValueType expectedValue
	)
	{
		GetPropertyMetadata(actualObject, propertySelector, out var actualValue, out var typeName, out var propertyName);

		Assert.AreEqual(expectedValue, actualValue, "{0}.{1}", typeName, propertyName);
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
	public static void AreEqual<ObjectType, KeyType, ValueType>
	(
		ObjectType actualObject,
		Expression<Func<ObjectType, IEnumerable<KeyValuePair<KeyType, ValueType>>>> propertySelector,
		IEnumerable<KeyValuePair<KeyType, ValueType>> expectedValue
	)
	{
		GetPropertyMetadata(actualObject, propertySelector, out var actualValue, out var typeName, out var propertyName);

		var comparer = new KeyValuePairEqualityComparer<KeyType, ValueType>
		(
			EqualityComparer<KeyType>.Default,
			EqualityComparer<ValueType>.Default
		);

		CollectionAssert.AreEquivalent(expectedValue, actualValue, comparer, "{0}.{1}", typeName, propertyName);
	}

	/// <summary>
	/// Tests if the value of the property of the <paramref name="actualObject"/> object equals to <paramref name="expectedValue"/>.
	/// </summary>
	/// <typeparam name="ObjectType">The type of the object.</typeparam>
	/// <param name="actualObject">The object.</param>
	/// <param name="propertySelector">An expression to get property from the <paramref name="actualObject"/>.</param>
	/// <param name="expectedValue">The expected value.</param>
	/// <exception cref="AssertFailedException">Thrown if <paramref name="expectedValue"/> is not equal to property value.</exception>
	public static void AreEqual<ObjectType>
	(
		ObjectType actualObject,
		Expression<Func<ObjectType, TelemetryOperation>> propertySelector,
		TelemetryOperation expectedValue
	)
	{
		GetPropertyMetadata(actualObject, propertySelector, out var actualValue, out var typeName, out var propertyName);

		if (ReferenceEquals(expectedValue, actualValue))
		{
			return;
		}

		Assert.AreEqual(expectedValue.Id, actualValue.Id, "{0}.{1}.{2}", typeName, propertyName, nameof(TelemetryOperation.Id));

		Assert.AreEqual(expectedValue.Name, actualValue.Name, "{0}.{1}.{2}", typeName, propertyName, nameof(TelemetryOperation.Name));

		Assert.AreEqual(expectedValue.ParentId, actualValue.ParentId, "{0}.{1}.{2}", typeName, propertyName, nameof(TelemetryOperation.ParentId));
	}

	/// <summary>
	/// Tests if the value of the property of the <paramref name="actualObject"/> object equals to <paramref name="expectedValue"/>.
	/// </summary>
	/// <typeparam name="ObjectType">The type of the object.</typeparam>
	/// <param name="actualObject">The object.</param>
	/// <param name="propertySelector">An expression to get property from the <paramref name="actualObject"/>.</param>
	/// <param name="expectedValue">The expected value.</param>
	/// <exception cref="AssertFailedException">Thrown if <paramref name="expectedValue"/> is not equal to property value.</exception>
	public static void AreEqual<ObjectType>
	(
		ObjectType actualObject,
		Expression<Func<ObjectType, MetricValueAggregation?>> propertySelector,
		MetricValueAggregation expectedValue
	)
	{
		GetPropertyMetadata(actualObject, propertySelector, out var actualValue, out var typeName, out var propertyName);

		if (ReferenceEquals(expectedValue, actualValue))
		{
			return;
		}

		if (actualValue == null)
		{
			Assert.Fail("{0}.{1}", typeName, propertyName);
		}

		Assert.AreEqual(expectedValue.Count, actualValue.Count, "{0}.{1}.{2}", typeName, propertyName, nameof(MetricValueAggregation.Count));

		Assert.AreEqual(expectedValue.Max, actualValue.Max, "{0}.{1}.{2}", typeName, propertyName, nameof(MetricValueAggregation.Max));

		Assert.AreEqual(expectedValue.Min, actualValue.Min, "{0}.{1}.{2}", typeName, propertyName, nameof(MetricValueAggregation.Min));
	}

	/// <summary>
	/// Tests if the <paramref name="testCondition"/> returns <c>true</c> and throws an exception if <c>false</c> is returned.
	/// </summary>
	/// <typeparam name="ObjectType">The type of the object.</typeparam>
	/// <typeparam name="ValueType">The type of the value.</typeparam>
	/// <param name="actualObject">The object.</param>
	/// <param name="propertySelector">An expression to get property from the <paramref name="actualObject"/>.</param>
	/// <param name="testCondition">The test condition.</param>
	/// <exception cref="AssertFailedException">Thrown if <paramref name="testCondition"/> returns <c>false</c>.</exception>
	public static void IsTrue<ObjectType, PropertyType>
	(
		ObjectType actualObject,
		Expression<Func<ObjectType, PropertyType>> propertySelector,
		Func<PropertyType, Boolean> testCondition
	)
	{
		GetPropertyMetadata(actualObject, propertySelector, out var actual, out var typeName, out var propertyName);

		var testResult = testCondition(actual);

		Assert.IsTrue(testResult, "{0}.{1}", typeName, propertyName);
	}

	#endregion

	#region Methods: private

	public static void GetPropertyMetadata<ObjectType, PropertyType>
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
