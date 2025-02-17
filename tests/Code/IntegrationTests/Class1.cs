// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Tests.Code.IntegrationTests;
internal class Class1
{
}
//[TestMethod]
//public async Task Combo()
//{
//	var cancellationToken = TestContext.CancellationTokenSource.Token;

//	var telemetry = TelemetryTracker.TrackRequestBegin(ActivitySpanId.CreateRandom().ToString(), new Uri($"tst:{nameof(TrackerIntegrationTests)}"));

//	await Simulate_Request_Success(cancellationToken);

//	await Simulate_Request_Fail(cancellationToken);

//	await Simulate_Request_SuccessWithException(cancellationToken);

//	await Simulate_Request_SuccessWithTraces(cancellationToken);

//	await Simulate_Dependency_AzureQueue_Success(cancellationToken);

//	TelemetryTracker.TrackRequestEnd(telemetry, true, "OK");

//	var trackResults = await TelemetryTracker.PublishAsync(cancellationToken);

//	AssertStandardSuccess(trackResults);

//	foreach (var result in trackResults.Cast<HttpTelemetryPublishResult>())
//	{
//		TelemetryTracker.TrackDependency(ActivitySpanId.CreateRandom().ToString(), result);
//	}

//	trackResults = await TelemetryTracker.PublishAsync(cancellationToken);

//	AssertStandardSuccess(trackResults);
//}

//[TestMethod]
//public async Task Dependency_AzureQueue_Success()
//{
//	var cancellationToken = TestContext.CancellationTokenSource.Token;

//	await Simulate_Dependency_AzureQueue_Success(cancellationToken);

//	var flushResult = await TelemetryTracker.PublishAsync(cancellationToken);

//	AssertStandardSuccess(flushResult);
//}

//[TestMethod]
//public async Task Exception_Single()
//{
//	var cancellationToken = TestContext.CancellationTokenSource.Token;

//	try
//	{
//		Simulate_ExceptionThrow(null);
//	}
//	catch (Exception exception)
//	{
//		TelemetryTracker.TrackException(exception, SeverityLevel.Critical);
//	}

//	var flushResult = await TelemetryTracker.PublishAsync(cancellationToken);

//	AssertStandardSuccess(flushResult);
//}

//[TestMethod]
//public async Task Exception_Chain()
//{
//	var cancellationToken = TestContext.CancellationTokenSource.Token;

//	try
//	{
//		Simulate_ExceptionThrow_WithInnerException(null);
//	}
//	catch (Exception exception)
//	{
//		TelemetryTracker.TrackException(exception, SeverityLevel.Critical);
//	};

//	var flushResult = await TelemetryTracker.PublishAsync(cancellationToken);

//	AssertStandardSuccess(flushResult);
//}

//[TestMethod]
//public async Task Request_Success()
//{
//	var cancellationToken = TestContext.CancellationTokenSource.Token;

//	await Simulate_Request_Success(cancellationToken);

//	var flushResult = await TelemetryTracker.PublishAsync(cancellationToken);

//	AssertStandardSuccess(flushResult);
//}

//[TestMethod]
//public async Task Request_Fail()
//{
//	var cancellationToken = TestContext.CancellationTokenSource.Token;

//	await Simulate_Request_Fail(cancellationToken);

//	var flushResult = await TelemetryTracker.PublishAsync(cancellationToken);

//	AssertStandardSuccess(flushResult);
//}

//[TestMethod]
//public async Task Request_SuccessWithException()
//{
//	var cancellationToken = TestContext.CancellationTokenSource.Token;

//	await Simulate_Request_SuccessWithException(cancellationToken);

//	var flushResult = await TelemetryTracker.PublishAsync(cancellationToken);

//	AssertStandardSuccess(flushResult);
//}

//[TestMethod]
//public async Task Trace()
//{
//	var cancellationToken = TestContext.CancellationTokenSource.Token;

//	var telemetry0 = new TraceTelemetry(DateTime.UtcNow, "The cow fly out.", SeverityLevel.Information)
//	{
//		Operation = TelemetryTracker.Operation
//	};

//	TelemetryTracker.Track(telemetry0);

//	var telemetry1 = new TraceTelemetry(DateTime.UtcNow, "The cow falls.", SeverityLevel.Critical)
//	{
//		Operation = TelemetryTracker.Operation
//	};

//	TelemetryTracker.Track(telemetry1);

//	var flushResults = await TelemetryTracker.PublishAsync(cancellationToken);

//	AssertStandardSuccess(flushResults);
//}

//private async Task Simulate_Dependency_AzureQueue_Success(CancellationToken cancellationToken = default)
//{
//	var time = DateTime.UtcNow;
//	var id = System.Diagnostics.ActivitySpanId.CreateRandom().ToString();
//	var uri = new Uri("https://test.queue.core.windows.net/commands/messages");

//	// simulate activity
//	await Task.Delay(random.Next(100), cancellationToken);

//	TelemetryTracker.TrackDependency(time, id, HttpMethod.Post, uri, HttpStatusCode.Created, DateTime.UtcNow.Subtract(time));
//}

//private async Task Simulate_Dependency_AzureMonitor_Success(CancellationToken cancellationToken = default)
//{
//	var time = DateTime.UtcNow;
//	var id = System.Diagnostics.ActivitySpanId.CreateRandom().ToString();
//	var uri = new Uri("https://eastus-8.in.applicationinsights.azure.com/v2.1/track");

//	// simulate activity
//	await Task.Delay(random.Next(100), cancellationToken);

//	TelemetryTracker.TrackDependency(time, id, HttpMethod.Post, uri, HttpStatusCode.OK, DateTime.UtcNow.Subtract(time));
//}

//private async Task Simulate_Request_Fail(CancellationToken cancellationToken = default)
//{
//	var id = System.Diagnostics.ActivitySpanId.CreateRandom().ToString();

//	var requestTelemetry = TelemetryTracker.TrackRequestBegin(id, new Uri($"tst:{nameof(TrackerIntegrationTests)}"));

//	// simulate activity
//	await Task.Delay(random.Next(100), cancellationToken);

//	TelemetryTracker.TrackRequestEnd(requestTelemetry, false, "Fail");
//}

//private async Task Simulate_Request_Success(CancellationToken cancellationToken = default)
//{
//	var id = ActivitySpanId.CreateRandom().ToString();

//	var requestTelemetry = TelemetryTracker.TrackRequestBegin(id, new Uri($"tst:{nameof(TrackerIntegrationTests)}"));

//	// simulate activity
//	await Task.Delay(random.Next(100), cancellationToken);

//	TelemetryTracker.TrackRequestEnd(requestTelemetry, true, "Success");
//}

//private async Task Simulate_Request_SuccessWithException(CancellationToken cancellationToken = default)
//{
//	var id = ActivitySpanId.CreateRandom().ToString();

//	var requestTelemetry = TelemetryTracker.TrackRequestBegin(id, new Uri($"tst:{nameof(TrackerIntegrationTests)}"));

//	// simulate activity
//	await Task.Delay(random.Next(100), cancellationToken);

//	try
//	{
//		Simulate_ExceptionThrow_WithInnerException(null);
//	}
//	catch (Exception exception)
//	{
//		TelemetryTracker.TrackException(exception);
//	}
//	finally
//	{
//		TelemetryTracker.TrackRequestEnd(requestTelemetry, true, "OK");
//	}
//}

//private async Task Simulate_Request_SuccessWithTraces(CancellationToken cancellationToken = default)
//{
//	var id = ActivitySpanId.CreateRandom().ToString();

//	var requestTelemetry = TelemetryTracker.TrackRequestBegin(id, new Uri($"tst:{nameof(TrackerIntegrationTests)}"));

//	// simulate activity
//	await Task.Delay(random.Next(100), cancellationToken);

//	var telemetry0 = new TraceTelemetry(DateTime.UtcNow, "The cow fly out.", SeverityLevel.Information)
//	{
//		Operation = TelemetryTracker.Operation
//	};

//	// simulate activity
//	await Task.Delay(random.Next(100), cancellationToken);

//	TelemetryTracker.Track(telemetry0);

//	var telemetry1 = new TraceTelemetry(DateTime.UtcNow, "The cow falls.", SeverityLevel.Critical)
//	{
//		Operation = TelemetryTracker.Operation
//	};

//	TelemetryTracker.Track(telemetry1);

//	// simulate activity
//	await Task.Delay(random.Next(100), cancellationToken);

//	TelemetryTracker.TrackRequestEnd(requestTelemetry, true, "OK");
//}