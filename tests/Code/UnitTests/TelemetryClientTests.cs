// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.TelemetryTests;

using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for <see cref="TelemetryClient"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class TelemetryClientTests
{
	#region Static Methods

	private static IEnumerable<KeyValuePair<String, String>>? GetExpectedTags
	(
		in TelemetryClient telemetryClient,
		in IReadOnlyCollection<KeyValuePair<String, String>>? tags
	)
	{
		var contextTags = telemetryClient.Context.IsEmpty() ? null : telemetryClient.Context.ToArray();

		var result = tags is null ? contextTags : (contextTags is null ? tags : [..contextTags, ..tags]);

		return result;
	}

	#endregion

	#region Fields

	private readonly TelemetryFactory factory;
	private readonly HttpTelemetryPublisherMock mockPublisher;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of <see cref="TelemetryClientTests"/> class.
	/// </summary>
	public TelemetryClientTests()
	{
		factory = new()
		{
			Measurements =
			[
				new("test", -1),
				new("test2", 0),
			],
			Properties =
			[
				new("test", "test"),
			],
			Tags =
			[
				new("test", "test"),
			]
		};

		mockPublisher = new();
	}

	#endregion

	#region Methods: Tests - Constructors

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
	public void Constructor_Overload_ThrowsArgumentNullException_IfPublishersIsNull()
	{
		// arrange
		TelemetryPublisher[] publishers = null!;

		// act
		var argumentNullException = Assert.ThrowsExactly<ArgumentNullException>
		(
			() => _ = new TelemetryClient(publishers)
		);
	}

	[TestMethod]
	public void Constructor_Overload_ThrowsArgumentException_IfPublishersCountIsZero()
	{
		// arrange
		TelemetryPublisher[] publishers = [];

		// act
		var argumentNullException = Assert.ThrowsExactly<ArgumentException>
		(
			() => _ = new TelemetryClient(publishers)
		);
	}

	[TestMethod]
	public void Constructor_Overload_ThrowsArgumentException_IfPublishersContainsNull()
	{
		// arrange
		TelemetryPublisher? nullPublisher = null;

		TelemetryPublisher[] publishers = [mockPublisher, nullPublisher!];

		// act
		var argumentNullException = Assert.ThrowsExactly<ArgumentException>
		(
			() => _ = new TelemetryClient(publishers)
		);
	}

	[TestMethod]
	public void Constructor_Initialize_Context()
	{
		// arrange
		var applicationVerValue = "1.1";
		var tags = new TelemetryTags()
		{
			ApplicationVer = applicationVerValue
		};

		{
			// act
			var telemetryClient = new TelemetryClient(mockPublisher);

			// arrange
			Assert.IsTrue(telemetryClient.Context.IsEmpty());
		}

		{
			// act
			var telemetryClient = new TelemetryClient(mockPublisher, tags);

			// arrange
			AssertHelper.PropertyEqualsTo(telemetryClient.Context, o => o.ApplicationVer, applicationVerValue);
		}

		{
			// act
			var telemetryClient = new TelemetryClient([mockPublisher, mockPublisher]);

			// arrange
			Assert.IsTrue(telemetryClient.Context.IsEmpty());
		}

		{
			// act
			var telemetryClient = new TelemetryClient([mockPublisher, mockPublisher], tags);

			// arrange
			AssertHelper.PropertyEqualsTo(telemetryClient.Context, o => o.ApplicationVer, applicationVerValue);
		}
	}

	#endregion

	#region Methods: Tests - PublishAsync

	[TestMethod]
	public async Task Method_PublishAsync_ShouldReturnEmptySuccess_WhenNoItems()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		// act
		var result = await telemetryClient.PublishAsync();

		// assert
		Assert.AreEqual(0, result.Length);
	}

	#endregion

	#region Methods: Tests - Add

	[TestMethod]
	public async Task Method_Add_ShouldEnqueueTelemetryItem()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);
		var telemetry = TelemetryFactory.Create_TraceTelemetry_Min("Test");

		// act
		telemetryClient.Add(telemetry);

		_ = await telemetryClient.PublishAsync();

		var actualResult = mockPublisher.Buffer.Dequeue() as TraceTelemetry;

		// assert
		Assert.IsNotNull(actualResult);

		Assert.AreEqual(telemetry, actualResult);
	}

	#endregion

	#region Methods: Tests - Activity Scope

	[TestMethod]
	public void Method_ActivityScopeBegin()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var activityId = TelemetryFactory.GetActivityId();

		// act
		var initial = telemetryClient.Context;

		telemetryClient.ActivityScopeBegin(activityId, out var scope);

		var actual = telemetryClient.Context;

		// assert
		Assert.AreEqual(initial, scope);

		Assert.AreNotEqual(initial, actual);

		Assert.AreEqual(activityId, actual.OperationParentId);
	}

	[TestMethod]
	public void ActivityScope()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var activityId = TelemetryFactory.GetActivityId();
		var operationIdTagValue = TelemetryFactory.GetOperationId();
		var operationNameTagValue = nameof(TelemetryClientTests);

		telemetryClient.Context = new()
		{
			OperationId = operationIdTagValue,
			OperationName = operationNameTagValue
		};

		// act
		var initial = telemetryClient.Context;

		telemetryClient.ActivityScopeBegin(activityId, out var scope);

		var actual = telemetryClient.Context;

		telemetryClient.ActivityScopeEnd(scope);

		var final = telemetryClient.Context;

		// assert
		Assert.AreEqual(initial, scope);

		Assert.AreNotEqual(initial, actual);

		Assert.AreEqual(activityId, actual.OperationParentId);

		Assert.AreEqual(initial, final);
	}

	[TestMethod]
	public void ActivityScope_Overload()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var expectedActivityId = TelemetryFactory.GetActivityId();

		// act
		telemetryClient.ActivityScopeBegin(() => expectedActivityId, out var time, out var timestamp, out var activityId, out var context);

		var actual = telemetryClient.Context;

		telemetryClient.ActivityScopeEnd(context, timestamp, out var duration);

		// assert
		Assert.IsTrue(time < DateTime.UtcNow);

		Assert.IsTrue(duration > TimeSpan.Zero);

		Assert.AreEqual(expectedActivityId, activityId);

		Assert.IsNotNull(actual);
	}

	#endregion

	#region Methods: Tests - Track

	[TestMethod]
	public async Task Method_TrackAvailability()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var inDuration = TimeSpan.FromSeconds(1);
		var inId = TelemetryFactory.GetActivityId();
		var inMessage = "ok";
		var inName = "inName";
		var inRunLocation = "test-server";
		var inSuccess = true;
		var inTime = DateTime.UtcNow;

		var expectedTags = GetExpectedTags(telemetryClient, factory.Tags);

		// act
		telemetryClient.TrackAvailability
		(
			inTime,
			inDuration,
			inId,
			inName,
			inMessage,
			inSuccess,
			inRunLocation,
			factory.Measurements,
			factory.Properties,
			factory.Tags
		);

		_ = await telemetryClient.PublishAsync();

		var actual = mockPublisher.Buffer.Dequeue() as AvailabilityTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.PropertyEqualsTo(actual, o => o.Duration, inDuration);
		AssertHelper.PropertyEqualsTo(actual, o => o.Id, inId);
		AssertHelper.PropertyEqualsTo(actual, o => o.Measurements, factory.Measurements);
		AssertHelper.PropertyEqualsTo(actual, o => o.Message, inMessage);
		AssertHelper.PropertyEqualsTo(actual, o => o.Name, inName);
		AssertHelper.PropertyEqualsTo(actual, o => o.Properties, factory.Properties);
		AssertHelper.PropertyEqualsTo(actual, o => o.RunLocation, inRunLocation);
		AssertHelper.PropertyEqualsTo(actual, o => o.Success, inSuccess);
		AssertHelper.PropertyEqualsTo(actual, o => o.Tags, expectedTags);
		AssertHelper.PropertyEqualsTo(actual, o => o.Time, inTime);
	}

	[TestMethod]
	public async Task Method_TrackDependencyHttp()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var inDuration = TimeSpan.FromSeconds(1);
		var inHttpMethod = HttpMethod.Post;
		var inId = TelemetryFactory.GetActivityId();
		var inStatusCode = HttpStatusCode.OK;
		var inSuccess = true;
		var inTime = DateTime.UtcNow;
		var inUri = new Uri("http://example.com");

		var expectedData = inUri.ToString();
		var expectedName = $"{inHttpMethod.Method} {inUri.AbsolutePath}";
		var expectedResultCode = inStatusCode.ToString();
		var expectedTags = GetExpectedTags(telemetryClient, factory.Tags);
		var expectedTarget = inUri.Host;
		var expectedType = TelemetryUtils.DetectDependencyTypeFromHttpUri(inUri);

		// act
		telemetryClient.TrackDependencyHttp
		(
			inTime,
			inDuration,
			inId,
			inHttpMethod,
			inUri,
			inStatusCode,
			inSuccess,
			factory.Measurements,
			factory.Properties,
			factory.Tags
		);

		_ = await telemetryClient.PublishAsync();

		var actual = mockPublisher.Buffer.Dequeue() as DependencyTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.PropertyEqualsTo(actual, o => o.Data, expectedData);
		AssertHelper.PropertyEqualsTo(actual, o => o.Duration, inDuration);
		AssertHelper.PropertyEqualsTo(actual, o => o.Id, inId);
		AssertHelper.PropertyEqualsTo(actual, o => o.Measurements, factory.Measurements);
		AssertHelper.PropertyEqualsTo(actual, o => o.Name, expectedName);
		AssertHelper.PropertyEqualsTo(actual, o => o.Properties, factory.Properties);
		AssertHelper.PropertyEqualsTo(actual, o => o.ResultCode, expectedResultCode);
		AssertHelper.PropertyEqualsTo(actual, o => o.Success, inSuccess);
		AssertHelper.PropertyEqualsTo(actual, o => o.Tags, expectedTags);
		AssertHelper.PropertyEqualsTo(actual, o => o.Target, expectedTarget);
		AssertHelper.PropertyEqualsTo(actual, o => o.Time, inTime);
		AssertHelper.PropertyEqualsTo(actual, o => o.Type, expectedType);
	}

	[TestMethod]
	public async Task Method_TrackDependencyInProc()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var inDuration = TimeSpan.FromSeconds(1);
		var inId = TelemetryFactory.GetActivityId();
		var inName = "inName";
		var inSuccess = true;
		var inTime = DateTime.UtcNow;
		var inTypeName = "Service";

		var expectedTags = GetExpectedTags(telemetryClient, factory.Tags);
		var expectedType = DependencyTypes.InProc + " | " + inTypeName;

		// act
		telemetryClient.TrackDependencyInProc
		(
			inTime,
			inDuration,
			inId, inName,
			inSuccess,
			inTypeName,
			factory.Measurements,
			factory.Properties,
			factory.Tags
		);

		_ = await telemetryClient.PublishAsync();

		var actual = mockPublisher.Buffer.Dequeue() as DependencyTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.PropertyEqualsTo(actual, o => o.Data, null);
		AssertHelper.PropertyEqualsTo(actual, o => o.Duration, inDuration);
		AssertHelper.PropertyEqualsTo(actual, o => o.Id, inId);
		AssertHelper.PropertyEqualsTo(actual, o => o.Measurements, factory.Measurements);
		AssertHelper.PropertyEqualsTo(actual, o => o.Name, inName);
		AssertHelper.PropertyEqualsTo(actual, o => o.Properties, factory.Properties);
		AssertHelper.PropertyEqualsTo(actual, o => o.ResultCode, null);
		AssertHelper.PropertyEqualsTo(actual, o => o.Success, inSuccess);
		AssertHelper.PropertyEqualsTo(actual, o => o.Tags, expectedTags);
		AssertHelper.PropertyEqualsTo(actual, o => o.Target, null);
		AssertHelper.PropertyEqualsTo(actual, o => o.Time, inTime);
		AssertHelper.PropertyEqualsTo(actual, o => o.Type, expectedType);
	}

	[TestMethod]
	public async Task Method_TrackDependencySql()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var inCommandText = "SELECT * FROM test";
		var inDataBase = "test";
		var inDataSource = "test.database.windows.net";
		var inDuration = TimeSpan.FromSeconds(1);
		var inId = TelemetryFactory.GetActivityId();
		var inResultCode = 0;
		var inTime = DateTime.UtcNow;

		var expectedName = $"{inDataSource} | {inDataBase}";
		var expectedResultCode = inResultCode == 0 ? null : inResultCode.ToString(CultureInfo.InvariantCulture);
		var expectedSuccess = inResultCode >= 0;
		var expectedTags = GetExpectedTags(telemetryClient, factory.Tags);
		var expectedTarget = $"{inDataSource} | {inDataBase}";

		// act
		telemetryClient.TrackDependencySql
		(
			inTime,
			inDuration,
			inId,
			inDataSource,
			inDataBase,
			inCommandText,
			inResultCode,
			factory.Measurements,
			factory.Properties,
			factory.Tags
		);

		_ = await telemetryClient.PublishAsync();

		var actual = mockPublisher.Buffer.Dequeue() as DependencyTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.PropertyEqualsTo(actual, o => o.Data, inCommandText);
		AssertHelper.PropertyEqualsTo(actual, o => o.Duration, inDuration);
		AssertHelper.PropertyEqualsTo(actual, o => o.Id, inId);
		AssertHelper.PropertyEqualsTo(actual, o => o.Measurements, factory.Measurements);
		AssertHelper.PropertyEqualsTo(actual, o => o.Name, expectedName);
		AssertHelper.PropertyEqualsTo(actual, o => o.Properties, factory.Properties);
		AssertHelper.PropertyEqualsTo(actual, o => o.ResultCode, expectedResultCode);
		AssertHelper.PropertyEqualsTo(actual, o => o.Success, expectedSuccess);
		AssertHelper.PropertyEqualsTo(actual, o => o.Tags, expectedTags);
		AssertHelper.PropertyEqualsTo(actual, o => o.Target, expectedTarget);
		AssertHelper.PropertyEqualsTo(actual, o => o.Time, inTime);
		AssertHelper.PropertyEqualsTo(actual, o => o.Type, DependencyTypes.SQL);
	}

	[TestMethod]
	public async Task Method_TrackEvent()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var inName = "test";

		var expectedTags = GetExpectedTags(telemetryClient, factory.Tags);
		var expectedTime = DateTime.UtcNow;

		// act
		telemetryClient.TrackEvent
		(
			inName,
			factory.Measurements,
			factory.Properties,
			factory.Tags
		);

		_ = await telemetryClient.PublishAsync();

		var actual = mockPublisher.Buffer.Dequeue() as EventTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.PropertyEqualsTo(actual, o => o.Measurements, factory.Measurements);
		AssertHelper.PropertyEqualsTo(actual, o => o.Name, inName);
		AssertHelper.PropertyEqualsTo(actual, o => o.Properties, factory.Properties);
		AssertHelper.PropertyEqualsTo(actual, o => o.Tags, expectedTags);
		AssertHelper.PropertyEvaluatesToTrue(actual, o => o.Time, p => p > expectedTime);
	}

	[TestMethod]
	public async Task Method_TrackException()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var inException = new Exception("Test exception");
		var inProblemId = Random.Shared.Next(1000).ToString(CultureInfo.InvariantCulture);
		var inSeverityLevel = SeverityLevel.Error;

		var expectedTags = GetExpectedTags(telemetryClient, factory.Tags);
		var expectedTime = DateTime.UtcNow;

		// act
		telemetryClient.TrackException
		(
			inException,
			inProblemId,
			inSeverityLevel,
			factory.Measurements,
			factory.Properties,
			factory.Tags
		);

		_ = await telemetryClient.PublishAsync();

		var actual = mockPublisher.Buffer.Dequeue() as ExceptionTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.PropertyEqualsTo(actual, o => o.Measurements, factory.Measurements);
		AssertHelper.PropertyEqualsTo(actual, o => o.ProblemId, inProblemId);
		AssertHelper.PropertyEqualsTo(actual, o => o.Properties, factory.Properties);
		AssertHelper.PropertyEqualsTo(actual, o => o.SeverityLevel, inSeverityLevel);
		AssertHelper.PropertyEqualsTo(actual, o => o.Tags, expectedTags);
		AssertHelper.PropertyEvaluatesToTrue(actual, o => o.Time, p => p > expectedTime);
	}

	[TestMethod]
	public async Task Method_TrackMetric()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var inName = "test";
		var inNamespace = "tests";
		var inValue = 6;

		var expectedTags = GetExpectedTags(telemetryClient, factory.Tags);
		var expectedTime = DateTime.UtcNow;

		// act
		telemetryClient.TrackMetric
		(
			inNamespace,
			inName,
			inValue,
			factory.Properties,
			factory.Tags
		);

		_ = await telemetryClient.PublishAsync();

		var actual = mockPublisher.Buffer.Dequeue() as MetricTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.PropertyEqualsTo(actual, o => o.Name, inName);
		AssertHelper.PropertyEqualsTo(actual, o => o.Namespace, inNamespace);
		AssertHelper.PropertyEqualsTo(actual, o => o.Properties, factory.Properties);
		AssertHelper.PropertyEqualsTo(actual, o => o.Tags, expectedTags);
		AssertHelper.PropertyEvaluatesToTrue(actual, o => o.Time, p => p > expectedTime);
		AssertHelper.PropertyEqualsTo(actual, o => o.Value, inValue);
	}

	[TestMethod]
	public async Task Method_TrackMetric_Overload()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var inName = "test";
		var inNamespace = "tests";
		var inValue = 6;
		var inValueAggregation = new MetricValueAggregation
		{
			Count = 3,
			Max = 3,
			Min = 1,
		};

		var expectedTags = GetExpectedTags(telemetryClient, factory.Tags);
		var expectedTime = DateTime.UtcNow;

		// act
		telemetryClient.TrackMetric
		(
			inNamespace,
			inName,
			inValue,
			inValueAggregation.Count,
			inValueAggregation.Max,
			inValueAggregation.Min,
			factory.Properties,
			factory.Tags
		);

		_ = await telemetryClient.PublishAsync();

		var actual = mockPublisher.Buffer.Dequeue() as MetricTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.PropertyEqualsTo(actual, o => o.Name, inName);
		AssertHelper.PropertyEqualsTo(actual, o => o.Namespace, inNamespace);
		AssertHelper.PropertyEqualsTo(actual, o => o.Properties, factory.Properties);
		AssertHelper.PropertyEqualsTo(actual, o => o.Tags, expectedTags);
		AssertHelper.PropertyEvaluatesToTrue(actual, o => o.Time, p => p > expectedTime);
		AssertHelper.PropertyEqualsTo(actual, o => o.Value, inValue);
		AssertHelper.PropertyEqualsTo(actual, o => o.ValueAggregation, inValueAggregation);
	}

	[TestMethod]
	public async Task Method_TrackPageView()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var inDuration = TimeSpan.FromSeconds(1);
		var inId = TelemetryFactory.GetActivityId();
		var inName = "inName";
		var inTime = DateTime.UtcNow;
		var inUrl = new Uri("https://gostas.dev");

		var expectedTags = GetExpectedTags(telemetryClient, factory.Tags);

		// act
		telemetryClient.TrackPageView
		(
			inTime,
			inDuration,
			inId,
			inName,
			inUrl,
			factory.Measurements,
			factory.Properties,
			factory.Tags
		);

		_ = await telemetryClient.PublishAsync();

		var actual = mockPublisher.Buffer.Dequeue() as PageViewTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.PropertyEqualsTo(actual, o => o.Duration, inDuration);
		AssertHelper.PropertyEqualsTo(actual, o => o.Id, inId);
		AssertHelper.PropertyEqualsTo(actual, o => o.Measurements, factory.Measurements);
		AssertHelper.PropertyEqualsTo(actual, o => o.Name, inName);
		AssertHelper.PropertyEqualsTo(actual, o => o.Properties, factory.Properties);
		AssertHelper.PropertyEqualsTo(actual, o => o.Tags, expectedTags);
		AssertHelper.PropertyEqualsTo(actual, o => o.Time, inTime);
		AssertHelper.PropertyEqualsTo(actual, o => o.Url, inUrl);
	}

	[TestMethod]
	public async Task Method_TrackRequest()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var inDuration = TimeSpan.FromSeconds(1);
		var inId = TelemetryFactory.GetActivityId();
		var inName = "inName";
		var inResponseCode = "1";
		var inSource = "test framework";
		var inSuccess = true;
		var inTime = DateTime.UtcNow;
		var inUrl = new Uri("tst:exe");

		var expectedTags = GetExpectedTags(telemetryClient, factory.Tags);

		// act
		telemetryClient.TrackRequest
		(
			inTime,
			inDuration,
			inId,
			inUrl,
			inResponseCode,
			inSuccess,
			inName,
			inSource,
			factory.Measurements,
			factory.Properties,
			factory.Tags
		);

		_ = await telemetryClient.PublishAsync();

		var actual = mockPublisher.Buffer.Dequeue() as RequestTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.PropertyEqualsTo(actual, o => o.Duration, inDuration);
		AssertHelper.PropertyEqualsTo(actual, o => o.Id, inId);
		AssertHelper.PropertyEqualsTo(actual, o => o.Measurements, factory.Measurements);
		AssertHelper.PropertyEqualsTo(actual, o => o.Name, inName);
		AssertHelper.PropertyEqualsTo(actual, o => o.Properties, factory.Properties);
		AssertHelper.PropertyEqualsTo(actual, o => o.ResponseCode, inResponseCode);
		AssertHelper.PropertyEqualsTo(actual, o => o.Success, inSuccess);
		AssertHelper.PropertyEqualsTo(actual, o => o.Tags, expectedTags);
		AssertHelper.PropertyEqualsTo(actual, o => o.Time, inTime);
		AssertHelper.PropertyEqualsTo(actual, o => o.Url, inUrl);
	}

	[TestMethod]
	public async Task Method_TrackTrace()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var inMessage = "test";
		var inSeverityLevel = SeverityLevel.Information;

		var expectedTags = GetExpectedTags(telemetryClient, factory.Tags);
		var expectedTime = DateTime.UtcNow;

		// act
		telemetryClient.TrackTrace
		(
			inMessage,
			inSeverityLevel,
			factory.Properties,
			factory.Tags
		);

		_ = await telemetryClient.PublishAsync();

		var actual = mockPublisher.Buffer.Dequeue() as TraceTelemetry;

		// assert
		Assert.IsNotNull(actual);

		AssertHelper.PropertyEqualsTo(actual, o => o.Message, inMessage);
		AssertHelper.PropertyEqualsTo(actual, o => o.Properties, factory.Properties);
		AssertHelper.PropertyEqualsTo(actual, o => o.SeverityLevel, inSeverityLevel);
		AssertHelper.PropertyEqualsTo(actual, o => o.Tags, expectedTags);
		AssertHelper.PropertyEvaluatesToTrue(actual, o => o.Time, p => p > expectedTime);
	}

	[TestMethod]
	public async Task Method_TrackTrace_WithinScope()
	{
		// arrange
		var telemetryClient = new TelemetryClient(mockPublisher);

		var activityId = TelemetryFactory.GetActivityId();

		var inMessage = "test";
		var inSeverityLevel = SeverityLevel.Information;

		// act
		telemetryClient.ActivityScopeBegin(activityId, out _);

		telemetryClient.TrackTrace(inMessage, inSeverityLevel);

		_ = await telemetryClient.PublishAsync();

		var actual = mockPublisher.Buffer.Dequeue() as TraceTelemetry;

		// assert
		Assert.IsNotNull(actual);

		Assert.IsNotNull(actual.Tags);

		var actualTags = new TelemetryTags(actual.Tags.ToDictionary(p => p.Key, p => p.Value));

		Assert.AreEqual(activityId, actualTags.OperationParentId);
	}

	#endregion
}
