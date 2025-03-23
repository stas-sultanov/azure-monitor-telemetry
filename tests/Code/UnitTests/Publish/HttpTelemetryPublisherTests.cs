// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System;
using System.Net.Http;

using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;
using Azure.Monitor.Telemetry.Tests;

/// <summary>
/// Tests for <see cref="HttpTelemetryPublisher"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed partial class HttpTelemetryPublisherTests
{
	#region Constants

	private const String mockValidIngestEndpoint = @"https://dc.in.applicationinsights.azure.com/";

	#endregion

	#region Static Fields

	private static readonly Uri ingestionEndpoint = new(mockValidIngestEndpoint);
	private static readonly Guid instrumentationKey = Guid.NewGuid();
	private static readonly TelemetryFactory telemetryFactory = new (nameof(HttpTelemetryPublisherTests));

	#endregion

	#region Methods: Tests

	[TestMethod]
	public void Constructor_ThrowsArgumentException_IfIngestionEndpointIsInvalid()
	{
		// arrange
		using var httpClient = new HttpClient();
		var ingestionEndpoint = new Uri("file://example.com");

		// act
		var argumentNullException = Assert.ThrowsExactly<ArgumentException>
		(
			() => _ = _ = new HttpTelemetryPublisher(httpClient, ingestionEndpoint, instrumentationKey)
		);

		// assert
		Assert.AreEqual(nameof(ingestionEndpoint), argumentNullException.ParamName);
	}

	[TestMethod]
	public void Constructor_ThrowsArgumentException_IfInstrumentationKeyIsEmpty()
	{
		// arrange
		var httpClient = new HttpClient();
		var instrumentationKey = Guid.Empty;

		// act
		var argumentNullException = Assert.ThrowsExactly<ArgumentException>
		(
			() => _ = _ = new HttpTelemetryPublisher(httpClient, ingestionEndpoint, instrumentationKey)
		);

		// assert
		Assert.AreEqual(nameof(instrumentationKey), argumentNullException.ParamName);
	}

	[TestMethod]
	public async Task Method_PublishAsync_WithoutAuthentication()
	{
		// arrange
		var time = DateTime.UtcNow;
		var httpClient = new HttpClient(new HttpMessageHandlerMock());
		var publisher = new HttpTelemetryPublisher(httpClient, ingestionEndpoint, instrumentationKey);
		var telemetryList = new[]
		{
			telemetryFactory.Create_TraceTelemetry_Min("Test")
		};

		// act
		var result = (await publisher.PublishAsync(telemetryList, null, CancellationToken.None)) as HttpTelemetryPublishResult;

		// assert
		Assert.IsNotNull(result);

		Assert.AreEqual(telemetryList.Length, result.Count, nameof(HttpTelemetryPublishResult.Count));

		Assert.IsTrue(result.Duration > TimeSpan.Zero, nameof(HttpTelemetryPublishResult.Duration));

		Assert.IsTrue(result.Response.Length > 0, nameof(HttpTelemetryPublishResult.Response));

		Assert.IsTrue(result.Success, nameof(HttpTelemetryPublishResult.Success));

		Assert.IsTrue(result.Time > time, nameof(HttpTelemetryPublishResult.Time));
	}

	[TestMethod]
	public async Task Method_PublishAsync_WithAuthToken()
	{
		// arrange
		var httpClient = new HttpClient(new HttpMessageHandlerMock());

		var publisher = new HttpTelemetryPublisher(httpClient, ingestionEndpoint, instrumentationKey, GetAccessToken, [new (TelemetryTagKeys.SessionId, "test"), new (TelemetryTagKeys.InternalAgentVersion, "test") ]);

		var telemetryList = new[]
		{
			telemetryFactory.Create_TraceTelemetry_Min("Test")
		};

		// act 1 - initiate publish, token will expire right after the call
		var result = await publisher.PublishAsync(telemetryList, null, CancellationToken.None);

		// assert 1
		Assert.IsTrue(result.Success);

		// act 2 - initiate publish, token will be re-requested
		result = await publisher.PublishAsync(telemetryList, null, CancellationToken.None);

		// assert 2
		Assert.IsTrue(result.Success);
	}

	private static Task<BearerToken> GetAccessToken(CancellationToken _)
	{
		var result = new BearerToken { ExpiresOn = DateTimeOffset.UtcNow, Value = "token " + HttpTelemetryPublisher.AuthorizationScope };

		return Task.FromResult(result);
	}

	#endregion
}
