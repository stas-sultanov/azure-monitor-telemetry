// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Tests;

using System;
using System.Net.Http;

using Azure.Monitor.Telemetry.Publish;

/// <summary>
/// Tests for <see cref="HttpTelemetryPublisher"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed partial class HttpTelemetryPublisherTests
{
	#region Constants

	private const String mockValidIngestEndpoint = "https://dc.in.applicationinsights.azure.com/";

	#endregion

	#region Static Fields

	private static readonly Uri ingestionEndpoint = new(mockValidIngestEndpoint);
	private static readonly Guid instrumentationKey = Guid.NewGuid();
	private static readonly TelemetryFactory telemetryFactory = new ();

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
			() => _ = new HttpTelemetryPublisher(httpClient, ingestionEndpoint, instrumentationKey)
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
			() => _ = new HttpTelemetryPublisher(httpClient, ingestionEndpoint, instrumentationKey)
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
			TelemetryFactory.Create_TraceTelemetry_Min("Test")
		};

		// act
		var result = (await publisher.PublishAsync(telemetryList, CancellationToken.None)) as HttpTelemetryPublishResult;

		// assert
		Assert.IsNotNull(result);

		AssertHelper.PropertyEqualsTo(result, e => e.Count, telemetryList.Length);

		AssertHelper.PropertyEvaluatesToTrue(result, e => e.Duration, value => value > TimeSpan.Zero);

		AssertHelper.PropertyEvaluatesToTrue(result, e => e.Success, value => value);

		AssertHelper.PropertyEvaluatesToTrue(result, e => e.Time, value => value > time);
	}

	[TestMethod]
	public async Task Method_PublishAsync_WithAuthToken()
	{
		// arrange
		var httpClient = new HttpClient(new HttpMessageHandlerMock());
		var publisher = new HttpTelemetryPublisher(httpClient, ingestionEndpoint, instrumentationKey, GetAccessToken);
		var telemetryList = new[]
		{
			TelemetryFactory.Create_TraceTelemetry_Min("Test")
		};

		// act 1 - initiate publish, token will expire right after the call
		var result = await publisher.PublishAsync(telemetryList, CancellationToken.None);

		// assert 1
		Assert.IsTrue(result.Success);

		// act 2 - initiate publish, token will be re-requested
		result = await publisher.PublishAsync(telemetryList, CancellationToken.None);

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
