// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using Azure.Monitor.Telemetry.Tests;

/// <summary>
/// Tests for <see cref="OperationContext"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class OperationContextTests
{
	[TestMethod]
	public void Constructor()
	{
		// arrange
		var id = "testId";
		var name = "testName";
		var parentId = "testParentId";
		var syntheticSource = "testSyntheticSource";

		// act
		var operation = new OperationContext
		{
			Id = id,
			Name = name,
			ParentId = parentId,
			SyntheticSource = syntheticSource
		};

		// assert
		AssertHelpers.PropertiesAreEqual(operation, id, name, parentId, syntheticSource);
	}

	[TestMethod]
	public void Constructor_Copy()
	{
		// arrange
		var newParentId = "newTestParentId";
		var operation = new OperationContext
		{
			Id = "testId",
			Name = "testName",
			ParentId = "testParentId",
			SyntheticSource = "testSyntheticSource"
		};

		// act
		var newOperation = operation with
		{
			ParentId = newParentId
		};

		// assert
		AssertHelpers.PropertiesAreEqual(newOperation, operation.Id, operation.Name, newParentId, operation.SyntheticSource);
	}
}