// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System.Net;

using Azure.Monitor.Telemetry.Mocks;
using Azure.Monitor.Telemetry.Publish;

/// <summary>
/// Tests for <see cref="Extensions"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class ExtensionsTests
{
	[TestMethod]
	public void Method_TrackDependency()
	{
		// arrange
		var operation = new TelemetryOperation (new Guid().ToString("N"), "Test" );
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryTracker = new TelemetryTracker(telemetryPublisher)
		{
			Operation = operation
		};
		var id = "test-id";
		var publishResult = new HttpTelemetryPublishResult
		(
			10,
			TimeSpan.FromSeconds(1),
			true,
			DateTime.UtcNow,
			new Uri("http://example.com"),
			HttpStatusCode.OK,
			""
		);

		// act
		telemetryTracker.TrackDependency(id, publishResult);
		telemetryTracker.PublishAsync().Wait();
		var actualResult = telemetryPublisher.Buffer.First() as DependencyTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		Assert.AreEqual(publishResult.Url.ToString(), actualResult.Data, nameof(DependencyTelemetry.Data));

		Assert.AreEqual(publishResult.Duration, actualResult.Duration, nameof(DependencyTelemetry.Duration));

		Assert.AreEqual(id, actualResult.Id, nameof(DependencyTelemetry.Id));

		Assert.IsNotNull(actualResult.Measurements, nameof(DependencyTelemetry.Measurements));

		Assert.AreEqual(nameof(HttpTelemetryPublishResult.Count), actualResult.Measurements[0].Key, nameof(DependencyTelemetry.Measurements));

		Assert.AreEqual(publishResult.Count, actualResult.Measurements[0].Value, nameof(DependencyTelemetry.Measurements));

		Assert.AreEqual("POST " + publishResult.Url.AbsolutePath, actualResult.Name, nameof(DependencyTelemetry.Name));

		Assert.AreEqual(operation, actualResult.Operation, nameof(DependencyTelemetry.Operation));

		Assert.AreEqual(publishResult.StatusCode.ToString(), actualResult.ResultCode, nameof(DependencyTelemetry.ResultCode));

		Assert.AreEqual(publishResult.Success, actualResult.Success, nameof(DependencyTelemetry.Success));

		Assert.AreEqual(publishResult.Url.Host, actualResult.Target, nameof(DependencyTelemetry.Target));

		Assert.AreEqual(DependencyType.AzureMonitor, actualResult.Type, nameof(DependencyTelemetry.Type));
	}

	[TestMethod]
	public void Method_TrackDependency_WithMeasurementsIsArray()
	{
		// arrange
		var id = "test-id";
		var measurements = new[] { new KeyValuePair<String, Double>("Number", 0) };
		var operation = new TelemetryOperation(new Guid().ToString("N"), "Test");
		var publishResult = new HttpTelemetryPublishResult
		(
			10,
			TimeSpan.FromSeconds(1),
			true,
			DateTime.UtcNow,
			new Uri("http://example.com"),
			HttpStatusCode.OK,
			""
		);
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryTracker = new TelemetryTracker(telemetryPublisher)
		{
			Operation = operation
		};

		// act
		telemetryTracker.TrackDependency(id, publishResult, measurements);
		telemetryTracker.PublishAsync().Wait();
		var actualResult = telemetryPublisher.Buffer.First() as DependencyTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		Assert.IsNotNull(actualResult.Measurements);

		Assert.AreEqual(2, actualResult.Measurements.Length);

		Assert.AreEqual(measurements[0], actualResult.Measurements[0]);

		Assert.AreEqual(nameof(HttpTelemetryPublishResult.Count), actualResult.Measurements[1].Key);

		Assert.AreEqual(publishResult.Count, actualResult.Measurements[1].Value);
	}

	[TestMethod]
	public void Method_TrackDependency_WithMeasurementsIsList()
	{
		// arrange
		var id = "test-id";
		var measurements = new KeyValuePair<String, Double> [] { new("Number", 0) };
		var operation = new TelemetryOperation (new Guid().ToString("N"), "Test" );
		var publishResult = new HttpTelemetryPublishResult
		(
			10,
			TimeSpan.FromSeconds(1),
			true,
			DateTime.UtcNow,
			new Uri("http://example.com"),
			HttpStatusCode.OK,
			""
		);
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryTracker = new TelemetryTracker(telemetryPublisher)
		{
			Operation = operation
		};

		// act
		telemetryTracker.TrackDependency(id, publishResult, measurements);
		telemetryTracker.PublishAsync().Wait();
		var actualResult = telemetryPublisher.Buffer.First() as DependencyTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		Assert.IsNotNull(actualResult.Measurements);

		Assert.AreEqual(2, actualResult.Measurements.Length);

		Assert.AreEqual(measurements[0], actualResult.Measurements[0]);

		Assert.AreEqual(nameof(HttpTelemetryPublishResult.Count), actualResult.Measurements[1].Key);

		Assert.AreEqual(publishResult.Count, actualResult.Measurements[1].Value);
	}
}