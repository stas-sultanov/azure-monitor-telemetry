// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System;
using System.Net;
using System.Threading.Tasks;

using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Mocks;
using Azure.Monitor.Telemetry.Tests;
using Azure.Monitor.Telemetry.Types;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for <see cref="TelemetryTracker"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class TelemetryTrackerTests
{
	#region Fields

	private readonly TelemetryFactory factory;
	private readonly HttpTelemetryPublisherMock publisher;
	private readonly TelemetryTracker tracker;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of <see cref="TelemetryTrackerTests"/> class.
	/// </summary>
	public TelemetryTrackerTests()
	{
		factory = new(nameof(TelemetryTrackerTests));
		publisher = new();
		tracker = new TelemetryTracker(publisher)
		{
			Operation = factory.Operation
		};
	}

	#endregion

	#region Methods: Tests PublishAsync

	[TestMethod]
	public async Task Method_PublishAsync_ShouldReturnEmptySuccess_WhenNoItems()
	{
		// arrange
		var telemetryTracker = new TelemetryTracker([]);

		// act
		var result = await telemetryTracker.PublishAsync();

		// assert
		Assert.AreEqual(0, result.Length);
	}

	#endregion

	#region Methods: Tests Add

	[TestMethod]
	public async Task Method_Add_ShouldEnqueueTelemetryItem()
	{
		// arrange
		var telemetry = factory.Create_TraceTelemetry_Min("Test");

		// act
		tracker.Add(telemetry);

		_ = await tracker.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as TraceTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		Assert.AreEqual(telemetry, actualResult);
	}

	#endregion

	#region Methods: Activity Scope

	[TestMethod]
	public void ActivityScopeBegin()
	{
		// arrange
		var originalOperation = tracker.Operation;
		var expectedId = TelemetryFactory.GetActivityId();

		// act
		tracker.ActivityScopeBegin(expectedId, out var actualOperation);

		var scopeOpeartion = tracker.Operation;

		tracker.ActivityScopeEnd(actualOperation);

		// assert
		AssertHelper.AreEqual(originalOperation, actualOperation);

		AssertHelper.PropertiesAreEqual(scopeOpeartion, actualOperation.Id, actualOperation.Name, expectedId);
	}

	[TestMethod]
	public void ActivityScopeBegin_Overload()
	{
		// arrange
		var originalOperation = tracker.Operation;
		var expectedId = TelemetryFactory.GetActivityId();

		// act
		tracker.ActivityScopeBegin(() => expectedId, out var time, out var timestamp, out var activityId, out var actualOperation);

		var scopeOpeartion = tracker.Operation;

		tracker.ActivityScopeEnd(actualOperation, timestamp, out var duration);

		// assert
		Assert.IsTrue(time < DateTime.UtcNow);

		Assert.IsTrue(duration > TimeSpan.Zero);

		Assert.AreEqual(expectedId, activityId);

		AssertHelper.AreEqual(originalOperation, actualOperation);

		AssertHelper.PropertiesAreEqual(scopeOpeartion, actualOperation.Id, actualOperation.Name, expectedId);
	}

	#endregion

	#region Methods: Tests Track

	[TestMethod]
	public async Task Method_TrackAvailability()
	{
		// arrange
		var id = TelemetryFactory.GetActivityId();
		var name = "name";
		var message = "ok";
		var time = DateTime.UtcNow;
		var duration = TimeSpan.FromSeconds(1);
		var success = true;
		var runLocation = "test-server";

		// act
		tracker.TrackAvailability(time, duration, id, name, message, success, runLocation, factory.Measurements, factory.Properties, factory.Tags);

		_ = await tracker.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as AvailabilityTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Operation, factory.Properties, factory.Tags);

		AssertHelper.PropertiesAreEqual(actualResult, actualResult.Duration, id, factory.Measurements, message, name, runLocation, success);
	}

	[TestMethod]
	public async Task Method_TrackDependency_With_HttpRequest()
	{
		// arrange
		var time = DateTime.UtcNow;
		var duration = TimeSpan.FromSeconds(1);
		var id = TelemetryFactory.GetActivityId();
		var httpMethod = HttpMethod.Post;
		var uri = new Uri("http://example.com");
		var statusCode = HttpStatusCode.OK;
		_ = TimeSpan.FromSeconds(1);

		// act
		tracker.TrackDependency(time, duration, id, httpMethod, uri, statusCode, true, factory.Measurements, factory.Properties, factory.Tags);

		_ = await tracker.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as DependencyTelemetry;

		var data = uri.ToString();
		var name = $"{httpMethod.Method} {uri.AbsolutePath}";
		var resultCode = statusCode.ToString();

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Operation, factory.Properties, factory.Tags);

		AssertHelper.PropertiesAreEqual(actualResult, data, actualResult.Duration, id, factory.Measurements, name, resultCode, true, uri.Host, DependencyType.HTTP);
	}

	[TestMethod]
	public async Task Method_TrackDependencyInProc()
	{
		// arrange
		var time = DateTime.UtcNow;
		var duration = TimeSpan.FromSeconds(1);
		var id = TelemetryFactory.GetActivityId();
		var name = "name";
		var typeName = "Service";
		var success = true;

		// act
		tracker.TrackDependencyInProc(time, duration, id, name, success, typeName, factory.Measurements, factory.Properties, factory.Tags);

		_ = await tracker.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as DependencyTelemetry;

		var type = DependencyType.InProc + " | " + typeName;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Operation, factory.Properties, factory.Tags);

		AssertHelper.PropertiesAreEqual(actualResult, null, actualResult.Duration, id, factory.Measurements, name, null, true, null, type);
	}

	[TestMethod]
	public async Task Method_TrackEvent()
	{
		// arrange
		var name = "test";

		// act
		tracker.TrackEvent(name, factory.Measurements, factory.Properties, factory.Tags);

		_ = await tracker.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as EventTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Operation, factory.Properties, factory.Tags);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Measurements, name);
	}

	[TestMethod]
	public async Task Method_TrackException()
	{
		// arrange
		var exception = new Exception("Test exception");
		var exceptions = exception.Convert();
		var severityLevel = SeverityLevel.Error;

		// act
		tracker.TrackException(exception, severityLevel, factory.Measurements, factory.Properties, factory.Tags);

		_ = await tracker.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as ExceptionTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Operation, factory.Properties, factory.Tags);

		AssertHelper.PropertiesAreEqual(actualResult, exceptions, factory.Measurements, severityLevel);
	}

	[TestMethod]
	public async Task Method_TrackMetric()
	{
		// arrange
		var name = "test";
		var @namespace = "tests";
		var value = 6;
		var valueAggregation = new MetricValueAggregation
		{
			Count = 3,
			Max = 3,
			Min = 1,
		};

		// act
		tracker.TrackMetric(@namespace, name, value, valueAggregation, factory.Properties, factory.Tags);

		_ = await tracker.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as MetricTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Operation, factory.Properties, factory.Tags);

		AssertHelper.PropertiesAreEqual(actualResult, name, @namespace, value, valueAggregation);
	}

	[TestMethod]
	public async Task Method_TrackPageView()
	{
		// arrange
		var time = DateTime.UtcNow;
		var duration = TimeSpan.FromSeconds(1);
		var id = TelemetryFactory.GetActivityId();
		var name = "name";
		var url = new Uri("https://gostas.dev");

		// act
		tracker.TrackPageView(time, duration, id, name, url, factory.Measurements, factory.Properties, factory.Tags);

		_ = await tracker.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as PageViewTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Operation, factory.Properties, factory.Tags);

		AssertHelper.PropertiesAreEqual(actualResult, actualResult.Duration, id, factory.Measurements, name, url);
	}

	[TestMethod]
	public async Task Method_TrackRequest()
	{
		// arrange
		var time = DateTime.UtcNow;
		var duration = TimeSpan.FromSeconds(1);
		var id = TelemetryFactory.GetActivityId();
		var url = new Uri("tst:exe");
		var responseCode = "1";
		var name = "name";
		var success = true;

		// act
		tracker.TrackRequest(time, duration, id, url, responseCode, success, name, factory.Measurements, factory.Properties, factory.Tags);

		_ = await tracker.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as RequestTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Operation, factory.Properties, factory.Tags);

		AssertHelper.PropertiesAreEqual(actualResult, actualResult.Duration, id, factory.Measurements, name, responseCode, success, url);
	}

	[TestMethod]
	public async Task Method_TrackTrace()
	{
		// arrange
		var message = "test";
		var severityLevel = SeverityLevel.Information;

		// act
		tracker.TrackTrace(message, severityLevel, factory.Properties, factory.Tags);

		_ = await tracker.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as TraceTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Operation, factory.Properties, factory.Tags);

		AssertHelper.PropertiesAreEqual(actualResult, message, severityLevel);
	}

	#endregion
}