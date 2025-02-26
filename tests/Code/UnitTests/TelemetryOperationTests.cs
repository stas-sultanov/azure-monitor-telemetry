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
		(
			id,
			name,
			parentId,
			syntheticSource
		);

		// assert
		AssertHelpers.PropertiesAreEqual(operation, id, name, parentId, syntheticSource);
	}
}