// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Mocks;
using Azure.Monitor.Telemetry.Models;
using Azure.Monitor.Telemetry.Tests;

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

	#region Methods: Tests Constructors

	[TestMethod]
	public void Constructor_ThrowsArgumentNullException_IfPublisherIsNull()
	{
		// arrange
		TelemetryPublisher? publisher = null;

		// act
		var argumentNullException = Assert.ThrowsExactly<ArgumentNullException>
		(
			() => _ = new TelemetryClient(publisher!)
		);
	}

	[TestMethod]
	public void Constructor_ThrowsArgumentNullException_IfPublishersContainsNull()
	{
		// arrange
		TelemetryPublisher? publisher = null;

		TelemetryPublisher[] publishers = [publisher!];

		// act
		var argumentNullException = Assert.ThrowsExactly<ArgumentException>
		(
			() => _ = new TelemetryClient(publishers)
		);
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
		var activityId = TelemetryFactory.GetActivityId();

		// act
		telemetryClient.ActivityScopeBegin(activityId, out var originalOperation);

		var scopeOperation = telemetryClient.Operation;

		telemetryClient.ActivityScopeEnd(originalOperation);

		var afterScopeOperation = telemetryClient.Operation;

		// assert
		AssertHelper.AreEqual(expectedOperation, originalOperation);

		AssertHelper.AreEqual(expectedOperation, afterScopeOperation);

		AssertHelper.AreEqual(scopeOperation, x => x.Id, originalOperation.Id);

		AssertHelper.AreEqual(scopeOperation, x => x.Name, originalOperation.Name);

		AssertHelper.AreEqual(scopeOperation, x => x.ParentId, activityId);
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

		AssertHelper.AreEqual(scopeOperation, x => x.Id, originalOperation.Id);

		AssertHelper.AreEqual(scopeOperation, x => x.Name, originalOperation.Name);

		AssertHelper.AreEqual(scopeOperation, x => x.ParentId, activityId);
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

		var actual = publisher.Buffer.Dequeue() as AvailabilityTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.AreEqual(actual, x => x.Duration, duration);

		AssertHelper.AreEqual(actual, x => x.Id, id);

		AssertHelper.AreEqual(actual, x => x.Measurements, factory.Measurements);

		AssertHelper.AreEqual(actual, x => x.Message, message);

		AssertHelper.AreEqual(actual, x => x.Name, name);

		AssertHelper.AreEqual(actual, x => x.Operation, factory.Operation);

		AssertHelper.AreEqual(actual, x => x.Properties, factory.Properties);

		AssertHelper.AreEqual(actual, x => x.RunLocation, runLocation);

		AssertHelper.AreEqual(actual, x => x.Success, success);

		AssertHelper.AreEqual(actual, x => x.Tags, factory.Tags);

		AssertHelper.AreEqual(actual, x => x.Time, time);
	}

	[TestMethod]
	public async Task Method_TrackDependencyHttp()
	{
		// arrange
		var time = DateTime.UtcNow;
		var duration = TimeSpan.FromSeconds(1);
		var id = TelemetryFactory.GetActivityId();
		var httpMethod = HttpMethod.Post;
		var uri = new Uri("http://example.com");
		var statusCode = HttpStatusCode.OK;
		var success = true;

		// act
		telemetryClient.TrackDependencyHttp(time, duration, id, httpMethod, uri, statusCode, success, factory.Measurements, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

		var actual = publisher.Buffer.Dequeue() as DependencyTelemetry;

		var data = uri.ToString();
		var name = $"{httpMethod.Method} {uri.AbsolutePath}";
		var resultCode = statusCode.ToString();
		var target = uri.Host;
		var type = TelemetryUtils.DetectDependencyTypeFromHttpUri(uri);

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.AreEqual(actual, x => x.Data, data);

		AssertHelper.AreEqual(actual, x => x.Duration, duration);

		AssertHelper.AreEqual(actual, x => x.Id, id);

		AssertHelper.AreEqual(actual, x => x.Measurements, factory.Measurements);

		AssertHelper.AreEqual(actual, x => x.Name, name);

		AssertHelper.AreEqual(actual, x => x.Operation, factory.Operation);

		AssertHelper.AreEqual(actual, x => x.Properties, factory.Properties);

		AssertHelper.AreEqual(actual, x => x.ResultCode, resultCode);

		AssertHelper.AreEqual(actual, x => x.Success, success);

		AssertHelper.AreEqual(actual, x => x.Tags, factory.Tags);

		AssertHelper.AreEqual(actual, x => x.Target, target);

		AssertHelper.AreEqual(actual, x => x.Time, time);

		AssertHelper.AreEqual(actual, x => x.Type, type);
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

		var actual = publisher.Buffer.Dequeue() as DependencyTelemetry;

		var type = DependencyTypes.InProc + " | " + typeName;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.AreEqual(actual, x => x.Data, null);

		AssertHelper.AreEqual(actual, x => x.Duration, duration);

		AssertHelper.AreEqual(actual, x => x.Id, id);

		AssertHelper.AreEqual(actual, x => x.Measurements, factory.Measurements);

		AssertHelper.AreEqual(actual, x => x.Name, name);

		AssertHelper.AreEqual(actual, x => x.Operation, factory.Operation);

		AssertHelper.AreEqual(actual, x => x.Properties, factory.Properties);

		AssertHelper.AreEqual(actual, x => x.ResultCode, null);

		AssertHelper.AreEqual(actual, x => x.Success, success);

		AssertHelper.AreEqual(actual, x => x.Tags, factory.Tags);

		AssertHelper.AreEqual(actual, x => x.Target, null);

		AssertHelper.AreEqual(actual, x => x.Time, time);

		AssertHelper.AreEqual(actual, x => x.Type, type);
	}

	[TestMethod]
	public async Task Method_TrackDependencySql()
	{
		// arrange
		var time = DateTime.UtcNow;
		var duration = TimeSpan.FromSeconds(1);
		var id = TelemetryFactory.GetActivityId();
		var dataSource = "test.database.windows.net";
		var database = "test";
		var commandText = "SELECT * FROM test";
		var resultCode = 0;

		// act
		telemetryClient.TrackDependencySql(time, duration, id, dataSource, database, commandText, resultCode, factory.Measurements, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

		var actual = publisher.Buffer.Dequeue() as DependencyTelemetry;
		var dataFullName = $"{dataSource} | {database}";
		var resultCodeAsString = resultCode == 0 ? null : resultCode.ToString(CultureInfo.InvariantCulture);
		var success = resultCode >= 0;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.AreEqual(actual, x => x.Data, commandText);

		AssertHelper.AreEqual(actual, x => x.Duration, duration);

		AssertHelper.AreEqual(actual, x => x.Id, id);

		AssertHelper.AreEqual(actual, x => x.Measurements, factory.Measurements);

		AssertHelper.AreEqual(actual, x => x.Name, dataFullName);

		AssertHelper.AreEqual(actual, x => x.Operation, factory.Operation);

		AssertHelper.AreEqual(actual, x => x.Properties, factory.Properties);

		AssertHelper.AreEqual(actual, x => x.ResultCode, resultCodeAsString);

		AssertHelper.AreEqual(actual, x => x.Success, success);

		AssertHelper.AreEqual(actual, x => x.Tags, factory.Tags);

		AssertHelper.AreEqual(actual, x => x.Target, dataFullName);

		AssertHelper.AreEqual(actual, x => x.Time, time);

		AssertHelper.AreEqual(actual, x => x.Type, DependencyTypes.SQL);
	}

	[TestMethod]
	public async Task Method_TrackEvent()
	{
		// arrange
		var name = "test";
		var time = DateTime.UtcNow;

		// act
		telemetryClient.TrackEvent(name, factory.Measurements, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

		var actual = publisher.Buffer.Dequeue() as EventTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.AreEqual(actual, x => x.Measurements, factory.Measurements);

		AssertHelper.AreEqual(actual, x => x.Name, name);

		AssertHelper.AreEqual(actual, x => x.Operation, factory.Operation);

		AssertHelper.AreEqual(actual, x => x.Properties, factory.Properties);

		AssertHelper.AreEqual(actual, x => x.Tags, factory.Tags);

		AssertHelper.IsTrue(actual, x => x.Time, p => p > time);
	}

	[TestMethod]
	public async Task Method_TrackException()
	{
		// arrange
		var exception = new Exception("Test exception");
		var exceptions = exception.ConvertExceptionToModel();
		var problemId = Random.Shared.Next(1000).ToString(CultureInfo.InvariantCulture);
		var severityLevel = SeverityLevel.Error;
		var time = DateTime.UtcNow;

		// act
		telemetryClient.TrackException(exception, problemId, severityLevel, factory.Measurements, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

		var actual = publisher.Buffer.Dequeue() as ExceptionTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.AreEqual(actual, x => x.Measurements, factory.Measurements);

		AssertHelper.AreEqual(actual, x => x.Operation, factory.Operation);

		AssertHelper.AreEqual(actual, x => x.ProblemId, problemId);

		AssertHelper.AreEqual(actual, x => x.Properties, factory.Properties);

		AssertHelper.AreEqual(actual, x => x.SeverityLevel, severityLevel);

		AssertHelper.AreEqual(actual, x => x.Tags, factory.Tags);

		AssertHelper.IsTrue(actual, x => x.Time, p => p > time);
	}

	[TestMethod]
	public async Task Method_TrackMetric()
	{
		// arrange
		var name = "test";
		var @namespace = "tests";
		var time = DateTime.UtcNow;
		var value = 6;

		// act
		telemetryClient.TrackMetric(@namespace, name, value, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

		var actual = publisher.Buffer.Dequeue() as MetricTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.AreEqual(actual, x => x.Name, name);

		AssertHelper.AreEqual(actual, x => x.Namespace, @namespace);

		AssertHelper.AreEqual(actual, x => x.Operation, factory.Operation);

		AssertHelper.AreEqual(actual, x => x.Properties, factory.Properties);

		AssertHelper.AreEqual(actual, x => x.Tags, factory.Tags);

		AssertHelper.IsTrue(actual, x => x.Time, p => p > time);

		AssertHelper.AreEqual(actual, x => x.Value, value);
	}

	[TestMethod]
	public async Task Method_TrackMetric_Overload()
	{
		// arrange
		var name = "test";
		var @namespace = "tests";
		var time = DateTime.UtcNow;
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

		var actual = publisher.Buffer.Dequeue() as MetricTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.AreEqual(actual, x => x.Name, name);

		AssertHelper.AreEqual(actual, x => x.Namespace, @namespace);

		AssertHelper.AreEqual(actual, x => x.Operation, factory.Operation);

		AssertHelper.AreEqual(actual, x => x.Properties, factory.Properties);

		AssertHelper.AreEqual(actual, x => x.Tags, factory.Tags);

		AssertHelper.IsTrue(actual, x => x.Time, p => p > time);

		AssertHelper.AreEqual(actual, x => x.Value, value);

		AssertHelper.AreEqual(actual, x => x.ValueAggregation, valueAggregation);
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

		var actual = publisher.Buffer.Dequeue() as PageViewTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.AreEqual(actual, x => x.Duration, duration);

		AssertHelper.AreEqual(actual, x => x.Id, id);

		AssertHelper.AreEqual(actual, x => x.Measurements, factory.Measurements);

		AssertHelper.AreEqual(actual, x => x.Name, name);

		AssertHelper.AreEqual(actual, x => x.Operation, factory.Operation);

		AssertHelper.AreEqual(actual, x => x.Properties, factory.Properties);

		AssertHelper.AreEqual(actual, x => x.Tags, factory.Tags);

		AssertHelper.AreEqual(actual, x => x.Time, time);

		AssertHelper.AreEqual(actual, x => x.Url, url);
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

		var actual = publisher.Buffer.Dequeue() as RequestTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.AreEqual(actual, x => x.Duration, duration);

		AssertHelper.AreEqual(actual, x => x.Id, id);

		AssertHelper.AreEqual(actual, x => x.Measurements, factory.Measurements);

		AssertHelper.AreEqual(actual, x => x.Name, name);

		AssertHelper.AreEqual(actual, x => x.Operation, factory.Operation);

		AssertHelper.AreEqual(actual, x => x.Properties, factory.Properties);

		AssertHelper.AreEqual(actual, x => x.ResponseCode, responseCode);

		AssertHelper.AreEqual(actual, x => x.Success, success);

		AssertHelper.AreEqual(actual, x => x.Tags, factory.Tags);

		AssertHelper.AreEqual(actual, x => x.Time, time);

		AssertHelper.AreEqual(actual, x => x.Url, url);
	}

	[TestMethod]
	public async Task Method_TrackTrace()
	{
		// arrange
		var message = "test";
		var severityLevel = SeverityLevel.Information;
		var time = DateTime.UtcNow;

		// act
		telemetryClient.TrackTrace(message, severityLevel, factory.Properties, factory.Tags);

		_ = await telemetryClient.PublishAsync();

		var actual = publisher.Buffer.Dequeue() as TraceTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.AreEqual(actual, x => x.Message, message);

		AssertHelper.AreEqual(actual, x => x.Operation, factory.Operation);

		AssertHelper.AreEqual(actual, x => x.Properties, factory.Properties);

		AssertHelper.AreEqual(actual, x => x.SeverityLevel, severityLevel);

		AssertHelper.AreEqual(actual, x => x.Tags, factory.Tags);

		AssertHelper.IsTrue(actual, x => x.Time, p => p > time);
	}

	[TestMethod]
	public async Task Method_TrackTrace_WithinScope()
	{
		// arrange
		_ = telemetryClient.Operation;
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
