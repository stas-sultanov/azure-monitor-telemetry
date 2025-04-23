// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.TelemetryTests;

using System;
using System.Net;

using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Models;
using Azure.Monitor.Telemetry.Publish;

/// <summary>
/// Tests for <see cref="TelemetryClientExtensions"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class TelemetryClientExtensionsTests
{
	#region Static Fields

	private static readonly Uri appInsightsTestUrl = new("https://myaccount.applicationinsights.azure.com");

	#endregion

	#region Methods: Tests

	[TestMethod]
	public void Method_TrackDependency()
	{
		// arrange
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryClient = new TelemetryClient(telemetryPublisher);

		var inId = "test-id";
		var inPublishResult = new HttpTelemetryPublishResult
		{
			Count = 10,
			Duration = TimeSpan.FromSeconds(1),
			Response = "",
			StatusCode = HttpStatusCode.OK,
			Success = true,
			Time = DateTime.UtcNow,
			Url = appInsightsTestUrl
		};

		var expectedName = $"POST {appInsightsTestUrl.AbsolutePath}";
		var expectedType = TelemetryUtils.DetectDependencyTypeFromHttpUri(appInsightsTestUrl);

		// act
		telemetryClient.TrackDependency(inId, inPublishResult);

		telemetryClient.PublishAsync().Wait();

		var actual = telemetryPublisher.Buffer.First() as DependencyTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.PropertyEqualsTo(actual, o => o.Name, expectedName);
		AssertHelper.PropertyEqualsTo(actual, o => o.Type, expectedType);
	}

	[TestMethod]
	public void Method_TrackDependency_WithMeasurements()
	{
		// arrange
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryClient = new TelemetryClient(telemetryPublisher);

		var count = 10;
		var measurement = new KeyValuePair<String, Double>("Number", 0);

		var inId = "test-id";
		var inPublishResult = new HttpTelemetryPublishResult
		{
			Count = count,
			Duration = TimeSpan.FromSeconds(1),
			Response = "OK",
			StatusCode = HttpStatusCode.OK,
			Success = true,
			Time = DateTime.UtcNow,
			Url = new Uri("https://myaccount.applicationinsights.azure.com")
		};

		var expectedMeasurements = (IEnumerable<KeyValuePair<String, Double>>)
		[
			measurement,
			new(nameof(HttpTelemetryPublishResult.Count), count)
		];

		// act
		telemetryClient.TrackDependency(inId, inPublishResult, [measurement]);

		telemetryClient.PublishAsync().Wait();

		var actual = telemetryPublisher.Buffer.First() as DependencyTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.PropertyEqualsTo(actual, o => o.Measurements, expectedMeasurements);
	}

	#endregion
}
