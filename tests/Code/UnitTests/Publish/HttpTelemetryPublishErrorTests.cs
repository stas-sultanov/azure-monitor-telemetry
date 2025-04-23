// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.TelemetryTests;

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
	#region Methods: Tests

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
		AssertHelper.PropertyEqualsTo(error, o => o.Index, index);

		AssertHelper.PropertyEqualsTo(error, o => o.Message, message);

		AssertHelper.PropertyEqualsTo(error, o => o.StatusCode, statusCode);
	}

	#endregion
}
