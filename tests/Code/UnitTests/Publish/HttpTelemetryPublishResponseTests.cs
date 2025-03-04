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
		IReadOnlyList<HttpTelemetryPublishError> errors =
		[
			new HttpTelemetryPublishError
			{
				Index = 0,
				Message = "Error message 1",
				StatusCode = HttpStatusCode.BadRequest
			},
			new HttpTelemetryPublishError
			{
				Index = 0,
				Message = "Error message 2",
				StatusCode = HttpStatusCode.InternalServerError
			}
		];
		var itemsAccepted = (UInt16)10;
		var itemsReceived = (UInt16)15;

		// act
		var response = new HttpTelemetryPublishResponse
		{
			Errors = errors,
			ItemsAccepted = itemsAccepted,
			ItemsReceived = itemsReceived
		};

		// assert
		Assert.AreEqual(errors, response.Errors, nameof(HttpTelemetryPublishResponse.Errors));

		Assert.AreEqual(itemsAccepted, response.ItemsAccepted, nameof(HttpTelemetryPublishResponse.ItemsAccepted));

		Assert.AreEqual(itemsReceived, response.ItemsReceived, nameof(HttpTelemetryPublishResponse.ItemsReceived));
	}
}