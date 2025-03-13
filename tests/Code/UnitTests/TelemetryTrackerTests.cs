// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Mocks;
using Azure.Monitor.Telemetry.Tests;
using Azure.Monitor.Telemetry.Types;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for <see cref="TelemetryClient"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class TelemetryClientTests
{
	#region Fields

	private readonly TelemetryFactory factory;
	private readonly HttpTelemetryPublisherMock publisher;
	private readonly TelemetryClient telemetryClient;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of <see cref="TelemetryClientTests"/> class.
	/// </summary>
	public TelemetryClientTests()
	{
		factory = new(nameof(TelemetryClientTests));
		publisher = new();
		telemetryClient = new TelemetryClient(publisher)
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
		var telemetryClient = new TelemetryClient([]);

		// act
		var result = await telemetryClient.PublishAsync();

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
		telemetryClient.Add(telemetry);

		_ = await telemetryClient.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as TraceTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		Assert.AreEqual(telemetry, actualResult);
	}

	#endregion

	#region Methods: Activity Scope

	[TestMethod]
	public void ActivityScope()
	{
		// arrange
		var expectedOperation = telemetryClient.Operation;
		var expectedId = TelemetryFactory.GetActivityId();

		// act
		telemetryClient.ActivityScopeBegin(expectedId, out var originalOperation);

		var scopeOperation = telemetryClient.Operation;

		telemetryClient.ActivityScopeEnd(originalOperation);

		var afterScopeOperation = telemetryClient.Operation;

		// assert
		AssertHelper.AreEqual(expectedOperation, originalOperation);

		AssertHelper.AreEqual(expectedOperation, afterScopeOperation);

		AssertHelper.PropertiesAreEqual(scopeOperation, originalOperation.Id, originalOperation.Name, expectedId);
	}

	[TestMethod]
	public void ActivityScope_Overload()
	{
		// arrange
		var originalOperation = telemetryClient.Operation;
		var expectedId = TelemetryFactory.GetActivityId();

		// act
		telemetryClient.ActivityScopeBegin(() => expectedId, out var time, out var timestamp, out var activityId, out var actualOperation);

		var scopeOperation = telemetryClient.Operation;

		telemetryClient.ActivityScopeEnd(actualOperation, timestamp, out var duration);

		// assert
		Assert.IsTrue(time < DateTime.UtcNow);

		Assert.IsTrue(duration > TimeSpan.Zero);

		Assert.AreEqual(expectedId, activityId);

		AssertHelper.AreEqual(originalOperation, actualOperation);

		AssertHelper.PropertiesAreEqual(scopeOperation, actualOperation.Id, actualOperation.Name, expectedId);
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
		telemetryClient.TrackAvailability(time, duration, id, name, message, success, runLocation, factory.Measurements, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

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
		telemetryClient.TrackDependency(time, duration, id, httpMethod, uri, statusCode, true, factory.Measurements, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as DependencyTelemetry;

		var data = uri.ToString();
		var name = $"{httpMethod.Method} {uri.AbsolutePath}";
		var resultCode = statusCode.ToString();

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Operation, factory.Properties, factory.Tags);

		AssertHelper.PropertiesAreEqual(actualResult, data, actualResult.Duration, id, factory.Measurements, name, resultCode, true, uri.Host, TelemetryDependencyTypes.HTTP);
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
		telemetryClient.TrackDependencyInProc(time, duration, id, name, success, typeName, factory.Measurements, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as DependencyTelemetry;

		var type = TelemetryDependencyTypes.InProc + " | " + typeName;

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
		telemetryClient.TrackEvent(name, factory.Measurements, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

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
		var problemId = Random.Shared.Next(1000).ToString(CultureInfo.InvariantCulture);
		var severityLevel = SeverityLevel.Error;

		// act
		telemetryClient.TrackException(exception, problemId, severityLevel, factory.Measurements, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as ExceptionTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Operation, factory.Properties, factory.Tags);

		AssertHelper.PropertiesAreEqual(actualResult, exceptions, factory.Measurements, problemId, severityLevel);
	}

	[TestMethod]
	public async Task Method_TrackMetric()
	{
		// arrange
		var name = "test";
		var @namespace = "tests";
		var value = 6;

		// act
		telemetryClient.TrackMetric(@namespace, name, value, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as MetricTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Operation, factory.Properties, factory.Tags);

		AssertHelper.PropertiesAreEqual(actualResult, name, @namespace, value);
	}

	[TestMethod]
	public async Task Method_TrackMetric_Overload()
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
		telemetryClient.TrackMetric(@namespace, name, value, valueAggregation.Count, valueAggregation.Max, valueAggregation.Min, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

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
		telemetryClient.TrackPageView(time, duration, id, name, url, factory.Measurements, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

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
		telemetryClient.TrackRequest(time, duration, id, url, responseCode, success, name, factory.Measurements, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

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
		telemetryClient.TrackTrace(message, severityLevel, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as TraceTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelper.PropertiesAreEqual(actualResult, factory.Operation, factory.Properties, factory.Tags);

		AssertHelper.PropertiesAreEqual(actualResult, message, severityLevel);
	}

	[TestMethod]
	public async Task Method_TrackTrace_WithinScope()
	{
		// arrange
		var expectedOperation = telemetryClient.Operation;
		var expectedId = TelemetryFactory.GetActivityId();
		var message = "test";
		var severityLevel = SeverityLevel.Information;

		// act
		telemetryClient.ActivityScopeBegin(expectedId, out var originalOperation);

		telemetryClient.TrackTrace(message, severityLevel);

		telemetryClient.ActivityScopeEnd(originalOperation);

		_ = await telemetryClient.PublishAsync();

		var actualResult = publisher.Buffer.Dequeue() as TraceTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		Assert.AreEqual(expectedId, actualResult.Operation.ParentId);
	}

	#endregion
}