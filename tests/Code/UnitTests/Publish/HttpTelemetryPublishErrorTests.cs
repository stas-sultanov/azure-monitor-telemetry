// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System;
using System.Net;

using Azure.Monitor.Telemetry.Publish;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for <see cref="HttpTelemetryPublishError"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class HttpTelemetryPublishErrorTests
{
	[TestMethod]
	public void Constructor()
	{
		// arrange
		var index = (UInt16)0;
		var message = "Error message";
		var statusCode = HttpStatusCode.BadRequest;

		// act
		var error = new HttpTelemetryPublishError
		{
			Index = index,
			Message = message,
			StatusCode = statusCode
		};

		// assert
		Assert.AreEqual(index, error.Index, nameof(HttpTelemetryPublishError.Index));

		Assert.AreEqual(message, error.Message, nameof(HttpTelemetryPublishError.Message));

		Assert.AreEqual(statusCode, error.StatusCode, nameof(HttpTelemetryPublishError.StatusCode));
	}
}
