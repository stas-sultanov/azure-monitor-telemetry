// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System;
using System.Net;

using Azure.Monitor.Telemetry.Publish;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for <see cref="HttpTelemetryPublishResponse"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class HttpTelemetryPublishResponseTests
{
	[TestMethod]
	public void Constructor()
	{
		// arrange
		var appId = "test-app-id";
		var errors = new[]
		{
			new HttpTelemetryPublishError(0, "Error message 1", HttpStatusCode.BadRequest),
			new HttpTelemetryPublishError(1, "Error message 2", HttpStatusCode.InternalServerError)
		};
		var itemsAccepted = (UInt16)10;
		var itemsReceived = (UInt16)15;

		// act
		var response = new HttpTelemetryPublishResponse(appId, errors, itemsAccepted, itemsReceived);

		// assert
		Assert.AreEqual(appId, response.AppId, nameof(HttpTelemetryPublishResponse.AppId));

		CollectionAssert.AreEqual(errors, response.Errors, nameof(HttpTelemetryPublishResponse.Errors));

		Assert.AreEqual(itemsAccepted, response.ItemsAccepted, nameof(HttpTelemetryPublishResponse.ItemsAccepted));

		Assert.AreEqual(itemsReceived, response.ItemsReceived, nameof(HttpTelemetryPublishResponse.ItemsReceived));
	}
}