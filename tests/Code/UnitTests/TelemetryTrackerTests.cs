// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Mocks;
using Azure.Monitor.Telemetry.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for <see cref="TelemetryTracker"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class TelemetryTrackerTests
{
	#region Fields

	private readonly KeyValuePair<String, Double> [] measurements = [new("m", 0)];
	private readonly TelemetryOperation operation = new()
	{
		Id = new Guid().ToString("N"),
		Name = "Test"
	};
	private readonly KeyValuePair<String, String> [] properties = [new("a", "b")];
	private readonly KeyValuePair<String, String> [] tags = [new(TelemetryTagKey.CloudRole, "role")];

	#endregion

	#region Methods: Tests Constructors

	[TestMethod]
	public void Constructor()
	{
		// arrange
		var operationId = Guid.NewGuid().ToString("N");
		var operation = new TelemetryOperation { Id = operationId };
		var tags = new KeyValuePair<String, String> []
		{
			new(TelemetryTagKey.CloudRole, "tester")
		};
		var telemetryPublisher = new HttpTelemetryPublisherMock();

		// act
		var telemetryTracker = new TelemetryTracker(telemetryPublisher, tags)
		{
			Operation = operation
		};

		// assert
		Assert.AreEqual(operation, telemetryTracker.Operation, nameof(telemetryTracker.Operation));
	}

	#endregion

	#region Tests: Method PublishAsync

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

	#region Tests: Method Add

	[TestMethod]
	public void Method_Add_ShouldEnqueueTelemetryItem()
	{
		// arrange
		var operationId = Guid.NewGuid().ToString("N");
		var operation = new TelemetryOperation { Id = operationId };
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryTracker = new TelemetryTracker(telemetryPublisher)
		{
			Operation = operation
		};
		var telemetry = new TraceTelemetry
		{
			Message = "test",
			Operation = operation,
			SeverityLevel = SeverityLevel.Information,
			Time = DateTime.UtcNow
		};

		// act
		telemetryTracker.Add(telemetry);
		telemetryTracker.PublishAsync().Wait();
		var actualResult = telemetryPublisher.Buffer.First() as TraceTelemetry;

		// assert
		Assert.AreEqual(telemetry, actualResult);
	}

	#endregion

	#region Tests: Method Track

	[TestMethod]
	public void Method_TrackAvailability()
	{
		// arrange
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryTracker = new TelemetryTracker(telemetryPublisher)
		{
			Operation = operation
		};
		var id = "test-id";
		var name = "name";
		var message = "ok";
		var time = DateTime.UtcNow;
		var duration = TimeSpan.FromSeconds(1);
		var success = true;
		var runLocation = "test-server";

		// act
		var operationInfo = telemetryTracker.TrackOperationBegin(()=>id);
		telemetryTracker.TrackAvailabilityEnd(operationInfo, name, message, success, runLocation, measurements, properties, tags);
		telemetryTracker.PublishAsync().Wait();
		var actualResult = telemetryPublisher.Buffer.First() as AvailabilityTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelpers.PropertiesAreEqual(actualResult, operation, properties, tags);

		AssertHelpers.PropertiesAreEqual(actualResult, actualResult.Duration, id, measurements, message, name, runLocation, success);
	}

	[TestMethod]
	public void Method_TrackDependency_With_HttpRequest()
	{
		// arrange
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryTracker = new TelemetryTracker(telemetryPublisher)
		{
			Operation = operation
		};
		var id = "test-id";
		var time = DateTime.UtcNow;
		var timestamp = Stopwatch.GetTimestamp();
		var httpMethod = HttpMethod.Post;
		var uri = new Uri("http://example.com");
		var statusCode = HttpStatusCode.OK;
		_ = TimeSpan.FromSeconds(1);

		// act
		var context = telemetryTracker.TrackOperationBegin(time, timestamp, id);
		telemetryTracker.TrackDependencyEnd(context, httpMethod, uri, statusCode, true, measurements, properties, tags);
		telemetryTracker.PublishAsync().Wait();
		var actualResult = telemetryPublisher.Buffer.First() as DependencyTelemetry;

		// assert
		var data = uri.ToString();
		var name = $"{httpMethod.Method} {uri.AbsolutePath}";
		var resultCode = statusCode.ToString();

		Assert.IsNotNull(actualResult);

		AssertHelpers.PropertiesAreEqual(actualResult, operation, properties, tags);

		AssertHelpers.PropertiesAreEqual(actualResult, data, actualResult.Duration, id, measurements, name, resultCode, true, uri.Host, DependencyType.HTTP);
	}

	[TestMethod]
	public void Method_TrackDependencyInProc()
	{
		// arrange
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryTracker = new TelemetryTracker(telemetryPublisher)
		{
			Operation = operation
		};
		var expectedId = Guid.NewGuid().ToString();
		var name = "name";
		var typeName = "Service";
		var duration = TimeSpan.FromSeconds(1);
		var success = true;

		// act
		var expectedParentId = telemetryTracker.Operation.ParentId;

		var operationInfo = telemetryTracker.TrackOperationBegin(() => expectedId);
		telemetryTracker.TrackDependencyInProcEnd(operationInfo, name, success, typeName, measurements, properties, tags);

		var actualParentId = telemetryTracker.Operation.ParentId;

		telemetryTracker.PublishAsync().Wait();
		var actualResult = telemetryPublisher.Buffer.First() as DependencyTelemetry;
		var type = DependencyType.InProc + " | " + typeName;

		// assert
		Assert.AreEqual(expectedParentId, actualParentId, "Operation.ParentId");

		Assert.IsNotNull(actualResult);

		AssertHelpers.PropertiesAreEqual(actualResult, operation, properties, tags);

		AssertHelpers.PropertiesAreEqual(actualResult, null, actualResult.Duration, expectedId, measurements, name, null, true, null, type);
	}

	[TestMethod]
	public void Method_TrackEvent()
	{
		// arrange
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var name = "test";
		var telemetryTracker = new TelemetryTracker(telemetryPublisher)
		{
			Operation = operation
		};

		// act
		telemetryTracker.TrackEvent(name, measurements, properties, tags);
		telemetryTracker.PublishAsync().Wait();
		var actualResult = telemetryPublisher.Buffer.First() as EventTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelpers.PropertiesAreEqual(actualResult, operation, properties, tags);

		AssertHelpers.PropertiesAreEqual(actualResult, measurements, name);
	}

	[TestMethod]
	public void Method_TrackException()
	{
		// arrange
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryTracker = new TelemetryTracker(telemetryPublisher)
		{
			Operation = operation
		};
		var exception = new Exception("Test exception");
		var severityLevel = SeverityLevel.Error;

		// act
		telemetryTracker.TrackException(exception, severityLevel, measurements, properties, tags);
		telemetryTracker.PublishAsync().Wait();
		var actualResult = telemetryPublisher.Buffer.First() as ExceptionTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelpers.PropertiesAreEqual(actualResult, operation, properties, tags);

		AssertHelpers.PropertiesAreEqual(actualResult, exception, measurements, severityLevel);
	}

	[TestMethod]
	public void Method_TrackMetric()
	{
		// arrange
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryTracker = new TelemetryTracker(telemetryPublisher)
		{
			Operation = operation
		};
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
		telemetryTracker.TrackMetric(@namespace, name, value, valueAggregation, properties, tags);
		telemetryTracker.PublishAsync().Wait();
		var actualResult = telemetryPublisher.Buffer.First() as MetricTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelpers.PropertiesAreEqual(actualResult, operation, properties, tags);

		AssertHelpers.PropertiesAreEqual(actualResult, name, @namespace, value, valueAggregation);
	}

	[TestMethod]
	public void Method_TrackRequest()
	{
		// arrange
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryTracker = new TelemetryTracker(telemetryPublisher)
		{
			Operation = operation
		};
		var expectedId = Guid.NewGuid().ToString();
		var url = new Uri("tst:exe");
		var responseCode = "1";
		var name = "name";
		var duration = TimeSpan.FromSeconds(1);
		var success = true;

		// act
		var expectedParentId = telemetryTracker.Operation.ParentId;
		var context = telemetryTracker.TrackOperationBegin(() => expectedId);
		telemetryTracker.TrackRequestEnd(context, url, responseCode, success, name, measurements, properties, tags);
		var actualParentId = telemetryTracker.Operation.ParentId;

		telemetryTracker.PublishAsync().Wait();
		var actualResult = telemetryPublisher.Buffer.First() as RequestTelemetry;

		// assert
		Assert.AreEqual(expectedParentId, actualParentId, "Operation.ParentId");

		Assert.IsNotNull(actualResult);

		AssertHelpers.PropertiesAreEqual(actualResult, operation, properties, tags);

		AssertHelpers.PropertiesAreEqual(actualResult, actualResult.Duration, expectedId, measurements, name, responseCode, success, url);
	}

	[TestMethod]
	public void Method_TrackTrace()
	{
		// arrange
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var message = "test";
		var severityLevel = SeverityLevel.Information;
		var telemetryTracker = new TelemetryTracker(telemetryPublisher)
		{
			Operation = operation
		};

		// act
		telemetryTracker.TrackTrace(message, severityLevel, properties, tags);
		telemetryTracker.PublishAsync().Wait();
		var actualResult = telemetryPublisher.Buffer.First() as TraceTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		AssertHelpers.PropertiesAreEqual(actualResult, operation, properties, tags);

		AssertHelpers.PropertiesAreEqual(actualResult, message, severityLevel);
	}

	#endregion
}