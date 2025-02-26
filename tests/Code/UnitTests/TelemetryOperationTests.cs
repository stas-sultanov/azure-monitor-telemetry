// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using Azure.Monitor.Telemetry.Tests;

/// <summary>
/// Tests for <see cref="TelemetryOperation"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class TelemetryOperationTests
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
		var operation = new TelemetryOperation
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