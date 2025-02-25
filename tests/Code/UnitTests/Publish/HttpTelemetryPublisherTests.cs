// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System;
using System.Net.Http;

using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;

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

	#region Methods: Tests

	[TestMethod]
	public void Constructor_ThrowsArgumentException_IfIngestionEndpointIsInvalid()
	{
		using var httpClient = new HttpClient();
		var ingestionEndpoint = new Uri("file://example.com");
		var instrumentationKey = Guid.NewGuid();

		// exception type
		var argumentNullException = Assert.ThrowsExactly<ArgumentException>
		(
			() => _ = _ = new HttpTelemetryPublisher(httpClient, ingestionEndpoint, instrumentationKey)
		);

		// parameter name
		Assert.AreEqual(nameof(ingestionEndpoint), argumentNullException.ParamName);
	}

	[TestMethod]
	public void Constructor_ThrowsArgumentException_IfInstrumentationKeyIsEmpty()
	{
		var httpClient = new HttpClient();
		var ingestionEndpoint = new Uri(mockValidIngestEndpoint);
		var instrumentationKey = Guid.Empty;

		// exception type
		var argumentNullException = Assert.ThrowsExactly<ArgumentException>
		(
			() => _ = _ = new HttpTelemetryPublisher(httpClient, ingestionEndpoint, instrumentationKey)
		);

		// parameter name
		Assert.AreEqual(nameof(instrumentationKey), argumentNullException.ParamName);
	}

	[TestMethod]
	public async Task Method_PublishAsync_WithoutAuthentication()
	{
		var httpClient = new HttpClient(new HttpMessageHandlerMock());
		var ingestionEndpoint = new Uri(mockValidIngestEndpoint);
		var instrumentationKey = Guid.NewGuid();

		var publisher = new HttpTelemetryPublisher(httpClient, ingestionEndpoint, instrumentationKey);

		var telemetryList = new[] { new TraceTelemetry(new OperationContext(), DateTime.UtcNow, @"test", SeverityLevel.Information) };
		var tags = Array.Empty<KeyValuePair<String, String>>();
		var cancellationToken = CancellationToken.None;

		var result = (await publisher.PublishAsync(telemetryList, tags, cancellationToken)) as HttpTelemetryPublishResult;

		Assert.IsNotNull(result);

		Assert.AreEqual(telemetryList.Length, result.Count, nameof(TelemetryPublishResult.Count));

		Assert.IsTrue(result.Duration > TimeSpan.Zero, nameof(TelemetryPublishResult.Duration));

		Assert.IsTrue(result.Success, nameof(TelemetryPublishResult.Success));

		Assert.IsTrue(result.Time > telemetryList[0].Time, nameof(TelemetryPublishResult.Time));
	}

	[TestMethod]
	public async Task Method_PublishAsync_WithAuthToken()
	{
		var httpClient = new HttpClient(new HttpMessageHandlerMock());
		var ingestionEndpoint = new Uri(mockValidIngestEndpoint);
		var instrumentationKey = Guid.NewGuid();

		static Task<BearerToken> getAccessToken(CancellationToken _)
		{
			return Task.FromResult(new BearerToken("token " + HttpTelemetryPublisher.AuthorizationScopes, DateTimeOffset.UtcNow));
		}

		var publisher = new HttpTelemetryPublisher(httpClient, ingestionEndpoint, instrumentationKey, getAccessToken, [new (TelemetryTagKey.SessionId, "test"), new (TelemetryTagKey.InternalAgentVersion, "test") ]);

		var telemetryList = new[] { new TraceTelemetry(new OperationContext(), DateTime.UtcNow, @"test", SeverityLevel.Information) };
		var tags = Array.Empty<KeyValuePair<String, String>>();
		var cancellationToken = CancellationToken.None;

		// initiate publish, token will expire right after the call
		var result = await publisher.PublishAsync(telemetryList, tags, cancellationToken);

		Assert.IsTrue(result.Success);

		// initiate publish, token will be re-requested
		result = await publisher.PublishAsync(telemetryList, tags, cancellationToken);

		Assert.IsTrue(result.Success);
	}

	#endregion
}
