// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System;

using Azure.Monitor.Telemetry.Dependency;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for <see cref="TelemetryTrackedHttpClientHandler"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class TelemetryTrackedHttpClientHandlerTests
{
	[TestMethod]
	public void Constructor_ThrowsException_WhenParameterIsNull()
	{
		// arrange
		static String getId()
		{
			return Guid.NewGuid().ToString();
		}

		var telemetryTracker = new TelemetryTracker();

		// act and assert
		_ = Assert.ThrowsException<ArgumentNullException>(() => new TelemetryTrackedHttpClientHandler(null, getId), "telemetryTracker");

		_ = Assert.ThrowsException<ArgumentNullException>(() => new TelemetryTrackedHttpClientHandler(telemetryTracker, null));

		using var x = new TelemetryTrackedHttpClientHandler(telemetryTracker, getId);
	}
}