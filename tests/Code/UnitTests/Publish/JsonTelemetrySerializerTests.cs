// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Azure.Monitor.Telemetry.Publish;
using Azure.Monitor.Telemetry.Tests;

/// <summary>
/// Tests for <see cref="JsonTelemetrySerializer"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class JsonTelemetrySerializerTests
{
	#region Types

	private sealed class UnknownTelemetry(DateTime time) : Telemetry
	{
		public TelemetryOperation Operation { get; set; } = new TelemetryOperation();
		public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; set; }
		public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; set; }
		public DateTime Time { get; init; } = time;
	}

	#endregion

	#region Static Fields

	private static readonly JsonSerializerOptions serializerOptionsWithEnumConverter;

	#endregion

	#region Static Constructors

	/// <summary>
	/// Initializes a new instance of <see cref="JsonTelemetrySerializerTests"/>.
	/// </summary>
	static JsonTelemetrySerializerTests()
	{
		var converter = new JsonStringEnumConverter();

		serializerOptionsWithEnumConverter = new JsonSerializerOptions();

		serializerOptionsWithEnumConverter.Converters.Add(converter);
	}

	#endregion

	#region Fields

	private readonly Uri testUrl = new ("https://gostas.dev");

	private readonly KeyValuePair<String, String> [] publisherTags =
	[
		new(TelemetryTagKey.InternalSdkVersion, "test"),
	];

	private readonly KeyValuePair<String, String> [] trackerTags =
	[
		new(TelemetryTagKey.CloudRole, "TestMachine"),
	];

	private readonly String instrumentationKey = Guid.NewGuid().ToString();
	private readonly TelemetryFactory telemetryFactory = new(nameof(HttpTelemetryPublisherTests));

	#endregion

	#region Methods: Tests

	[TestMethod]
	public void Method_Serialize_AvailabilityTelemetry()
	{
		// arrange
		var expectedName = @"AppAvailabilityResults";
		var expectedType = @"AvailabilityData";

		var telemetry = telemetryFactory.Create_AvailabilityTelemetry_Max("Check");

		// act
		var rootElement = SerializeAndDeserialize(instrumentationKey, telemetry, trackerTags, publisherTags);

		// assert
		var expectedTags = GetTags(telemetry, trackerTags, publisherTags);

		var jsonElement = DeserializeAndAssertBase(rootElement, expectedName, telemetry.Time, instrumentationKey, expectedTags, expectedType);

		var duration = GetDuration(jsonElement);

		var id = GetId(jsonElement);

		var measurements = GetMeasurements(jsonElement);

		var message = GetMessage(jsonElement);

		var name = GetName(jsonElement);

		var runLocation = GetRunLocation(jsonElement);

		var success = GetSuccess(jsonElement);

		AssertHelper.PropertiesAreEqual(telemetry, duration, id, measurements, message, name, runLocation, success);
	}

	[TestMethod]
	public void Method_Serialize_DependencyTelemetry()
	{
		// arrange
		var expectedName = @"AppDependencies";
		var expectedType = @"RemoteDependencyData";

		var telemetry = telemetryFactory.Create_DependencyTelemetry_Max("Storage",  testUrl );

		// act
		var rootElement = SerializeAndDeserialize(instrumentationKey, telemetry, trackerTags, publisherTags);

		// assert
		var expectedTags = GetTags(telemetry, trackerTags, publisherTags);

		var jsonElement = DeserializeAndAssertBase(rootElement, expectedName, telemetry.Time, instrumentationKey, expectedTags, expectedType);

		var data = GetData(jsonElement);

		var duration = GetDuration(jsonElement);

		var id = GetId(jsonElement);

		var measurements = GetMeasurements(jsonElement);

		var name = GetName(jsonElement);

		var resultCode = GetResultCode(jsonElement);

		var success = GetSuccess(jsonElement);

		var target = GetTarget(jsonElement);

		var type = GetType(jsonElement);

		AssertHelper.PropertiesAreEqual(telemetry, data, duration, id, measurements, name, resultCode, success, target, type);
	}

	[TestMethod]
	public void Method_Serialize_EventTelemetry()
	{
		// arrange
		var expectedName = @"AppEvents";
		var expectedType = @"EventData";

		var telemetry = telemetryFactory.Create_EventTelemetry_Max("Check");

		// act
		var rootElement = SerializeAndDeserialize(instrumentationKey, telemetry, trackerTags, publisherTags);

		// assert
		var expectedTags = GetTags(telemetry, trackerTags, publisherTags);

		var jsonElement = DeserializeAndAssertBase(rootElement, expectedName, telemetry.Time, instrumentationKey, expectedTags, expectedType);

		var measurements = GetMeasurements(jsonElement);

		var name = GetName(jsonElement);

		AssertHelper.PropertiesAreEqual(telemetry, measurements, name);
	}

	[TestMethod]
	public void Method_Serialize_ExceptionTelemetry()
	{
		// arrange
		var expectedName = @"AppExceptions";
		var expectedType = @"ExceptionData";

		var telemetry = telemetryFactory.Create_ExceptionTelemetry_Max();

		// act
		var rootElement = SerializeAndDeserialize(instrumentationKey, telemetry, trackerTags, publisherTags);

		// assert
		var expectedTags = GetTags(telemetry, trackerTags, publisherTags);

		var jsonElement = DeserializeAndAssertBase(rootElement, expectedName, telemetry.Time, instrumentationKey, expectedTags, expectedType);

		var measurements = GetMeasurements(jsonElement);

		var severityLevel = GetSeverityLevel(jsonElement);

		AssertHelper.PropertiesAreEqual(telemetry, [], measurements, severityLevel);
	}

	[TestMethod]
	public void Method_Serialize_MetricTelemetry()
	{
		// arrange
		var expectedName = @"AppMetrics";
		var expectedType = @"MetricData";

		var aggregation = new MetricValueAggregation()
		{
			Count = 3,
			Min = 1,
			Max = 3
		};
		var telemetry = telemetryFactory.Create_MetricTelemetry_Max("tests", "count", 6, aggregation);

		// act
		var rootElement = SerializeAndDeserialize(instrumentationKey, telemetry, trackerTags, publisherTags);

		// assert
		var expectedTags = GetTags(telemetry, trackerTags, publisherTags);

		var jsonElement = DeserializeAndAssertBase(rootElement, expectedName, telemetry.Time, instrumentationKey, expectedTags, expectedType);

		var metricJsonElement = jsonElement.GetProperty(@"metrics")[0];

		var count = metricJsonElement.GetProperty(@"count").Deserialize<Int32>();

		var max = metricJsonElement.GetProperty(@"max").Deserialize<Double>();

		var min = metricJsonElement.GetProperty(@"min").Deserialize<Double>();

		var name = GetName(metricJsonElement);

		var @namespace = metricJsonElement.GetProperty(@"ns").Deserialize<String>();

		var value = metricJsonElement.GetProperty(@"value").Deserialize<Double>();

		AssertHelper.PropertiesAreEqual(telemetry, name, @namespace, value, new MetricValueAggregation() { Count = count, Max = max, Min = min });
	}

	[TestMethod]
	public void Method_Serialize_PageViewTelemetry()
	{
		// arrange
		var expectedName = @"AppPageViews";
		var expectedType = @"PageViewData";

		var telemetry = telemetryFactory.Create_PageViewTelemetry_Max("Main", testUrl);

		// act
		var rootElement = SerializeAndDeserialize(instrumentationKey, telemetry, trackerTags, publisherTags);

		// assert
		var expectedTags = GetTags(telemetry, trackerTags, publisherTags);

		var jsonElement = DeserializeAndAssertBase(rootElement, expectedName, telemetry.Time, instrumentationKey, expectedTags, expectedType);

		var duration = GetDuration(jsonElement);

		var id = GetId(jsonElement);

		var measurements = GetMeasurements(jsonElement);

		var name = GetName(jsonElement);

		var url = GetUrl(jsonElement);

		AssertHelper.PropertiesAreEqual(telemetry, duration, id, measurements, name, url);
	}

	[TestMethod]
	public void Method_Serialize_RequestTelemetry()
	{
		// arrange
		var expectedName = @"AppRequests";
		var expectedType = @"RequestData";

		var telemetry = telemetryFactory.Create_RequestTelemetry_Max("GetMain", testUrl);

		// act
		var rootElement = SerializeAndDeserialize(instrumentationKey, telemetry, trackerTags, publisherTags);

		// assert
		var expectedTags = GetTags(telemetry, trackerTags, publisherTags);

		var jsonElement = DeserializeAndAssertBase(rootElement, expectedName, telemetry.Time, instrumentationKey, expectedTags, expectedType);

		var duration = GetDuration(jsonElement);

		var id = GetId(jsonElement);

		var measurements = GetMeasurements(jsonElement);

		var name = GetName(jsonElement);

		var responseCode = GetResponseCode(jsonElement);

		var success = GetSuccess(jsonElement);

		var url = GetUrl(jsonElement);

		AssertHelper.PropertiesAreEqual(telemetry, duration, id, measurements, name, responseCode, success, url);
	}

	[TestMethod]
	public void Method_Serialize_TraceTelemetry()
	{
		// arrange
		var expectedName = @"AppTraces";
		var expectedType = @"MessageData";

		var telemetry = telemetryFactory.Create_TraceTelemetry_Max("Test");

		// act
		var rootElement = SerializeAndDeserialize(instrumentationKey, telemetry, trackerTags, publisherTags);

		// assert
		var expectedTags = GetTags(telemetry, trackerTags, publisherTags);

		var jsonElement = DeserializeAndAssertBase(rootElement, expectedName, telemetry.Time, instrumentationKey, expectedTags, expectedType);

		var message = GetMessage(jsonElement);

		var severityLevel = GetSeverityLevel(jsonElement);

		AssertHelper.PropertiesAreEqual(telemetry, message, severityLevel);
	}

	[TestMethod]
	public void Method_Serialize_UnknownTelemetry()
	{
		// arrange
		var instrumentationKey = Guid.NewGuid().ToString();
		var telemetry = new UnknownTelemetry(DateTime.UtcNow);

		// act
		var memoryStream = new MemoryStream();

		using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 32768, true))
		{
			JsonTelemetrySerializer.Serialize(streamWriter, instrumentationKey, telemetry, null, null);
		}

		// assert
		Assert.AreEqual(3, memoryStream.Position);
	}

	#endregion

	#region Methods: Helpers

	private static JsonElement SerializeAndDeserialize
	(
		String instrumentationKey,
		Telemetry telemetry,
		KeyValuePair<String, String>[] trackerTags,
		KeyValuePair<String, String>[] publisherTags
	)
	{
		var memoryStream = new MemoryStream();

		using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 32768, true))
		{
			JsonTelemetrySerializer.Serialize(streamWriter, instrumentationKey, telemetry, trackerTags, publisherTags);
		}

		memoryStream.Position = 0;

		using var streamReader = new StreamReader(memoryStream);

		var streamAsString = streamReader.ReadToEnd();

		var result = JsonSerializer.Deserialize<JsonDocument>(streamAsString);

		Assert.IsNotNull(result);

		return result.RootElement;
	}

	private static JsonElement DeserializeAndAssertBase
	(
		JsonElement jsonElement,
		String expectedName,
		DateTime expectedTime,
		String expectedInstrumentationKey,
		KeyValuePair<String, String>[] expectedTags,
		String expectedBaseType
	)
	{
		DeserializeAndAssert(jsonElement, "name", expectedName);

		DeserializeAndAssert(jsonElement, "time", expectedTime);

		DeserializeAndAssert(jsonElement, "iKey", expectedInstrumentationKey);

		var actualTags = jsonElement.GetProperty(@"tags").Deserialize<Dictionary<String, String>>();

		Assert.IsNotNull(actualTags, "tags");

		Assert.IsTrue(expectedTags.Length <= actualTags.Count, "Tags count");

		foreach (var expectedTag in expectedTags)
		{
			if (!actualTags.TryGetValue(expectedTag.Key, out var actualValue))
			{
				Assert.Fail("tags does not contain key: {0}", expectedTag.Key);
			}

			Assert.AreEqual(expectedTag.Value, actualValue, $"tag {expectedTag.Key} value");
		}

		var childElement = jsonElement.GetProperty(@"data");

		DeserializeAndAssert(childElement, "baseType", expectedBaseType);

		var dataElement = childElement.GetProperty(@"baseData");

		return dataElement;
	}

	private static KeyValuePair<String, String>[] GetTags(Telemetry telemetry, KeyValuePair<String, String>[] trackerTags, KeyValuePair<String, String>[] publisherTags)
	{
		var tags = new List<KeyValuePair<String, String>>();

		if (telemetry.Operation != null)
		{
			if (!String.IsNullOrWhiteSpace(telemetry.Operation.Id))
			{
				tags.Add(new KeyValuePair<String, String>(TelemetryTagKey.OperationId, telemetry.Operation.Id));
			}

			if (!String.IsNullOrWhiteSpace(telemetry.Operation.Name))
			{
				tags.Add(new KeyValuePair<String, String>(TelemetryTagKey.OperationName, telemetry.Operation.Name));
			}

			if (!String.IsNullOrWhiteSpace(telemetry.Operation.ParentId))
			{
				tags.Add(new KeyValuePair<String, String>(TelemetryTagKey.OperationParentId, telemetry.Operation.ParentId));
			}
		}

		tags.AddRange(trackerTags);

		tags.AddRange(publisherTags);

		var result = tags.ToArray();

		return result;
	}

	private static void DeserializeAndAssert<ElementType>
	(
		JsonElement jsonElement,
		String propertyName,
		ElementType? expectedValue,
		JsonSerializerOptions? options = null
	)
	{
		var actualValue = jsonElement.GetProperty(propertyName).Deserialize<ElementType>(options);

		Assert.AreEqual(expectedValue, actualValue, propertyName);
	}

	#endregion

	#region Methods: Helpers Get

	private static String? GetData(JsonElement jsonElement)
	{
		return jsonElement.GetProperty(@"data").Deserialize<String>();
	}

	private static TimeSpan GetDuration(JsonElement jsonElement)
	{
		return jsonElement.GetProperty(@"duration").Deserialize<TimeSpan>();
	}

	private static String? GetId(JsonElement jsonElement)
	{
		return jsonElement.GetProperty(@"id").Deserialize<String>();
	}

	private static String? GetMessage(JsonElement jsonElement)
	{
		return jsonElement.GetProperty(@"message").Deserialize<String>();
	}

	private static KeyValuePair<String, Double>[] GetMeasurements(JsonElement jsonElement)
	{
		var result = jsonElement.GetProperty(@"measurements").Deserialize<Dictionary<String, Double>>();

		return result == null ? [] : [.. result];
	}

	private static String? GetName(JsonElement jsonElement)
	{
		return jsonElement.GetProperty(@"name").Deserialize<String>();
	}

	private static String? GetResultCode(JsonElement jsonElement)
	{
		return jsonElement.GetProperty(@"resultCode").Deserialize<String>();
	}

	private static String? GetResponseCode(JsonElement jsonElement)
	{
		return jsonElement.GetProperty(@"responseCode").Deserialize<String>();
	}

	private static String? GetRunLocation(JsonElement jsonElement)
	{
		return jsonElement.GetProperty(@"runLocation").Deserialize<String>();
	}

	private static SeverityLevel? GetSeverityLevel(JsonElement jsonElement)
	{
		return jsonElement.GetProperty(@"severityLevel").Deserialize<SeverityLevel>(serializerOptionsWithEnumConverter);
	}

	private static Boolean GetSuccess(JsonElement jsonElement)
	{
		return jsonElement.GetProperty(@"success").Deserialize<Boolean>();
	}

	private static String? GetTarget(JsonElement jsonElement)
	{
		return jsonElement.GetProperty(@"target").Deserialize<String>();
	}

	private static String? GetType(JsonElement jsonElement)
	{
		return jsonElement.GetProperty(@"type").Deserialize<String>();
	}

	private static Uri? GetUrl(JsonElement jsonElement)
	{
		return jsonElement.GetProperty(@"url").Deserialize<Uri>();
	}

	#endregion
}