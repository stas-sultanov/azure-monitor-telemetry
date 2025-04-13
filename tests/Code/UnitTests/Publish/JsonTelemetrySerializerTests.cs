// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Tests;

using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Azure.Monitor.Telemetry.Models;
using Azure.Monitor.Telemetry.Publish;

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
		public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; set; }
		public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; set; }
		public DateTime Time { get; init; } = time;
	}

	#endregion

	#region Fields

	private readonly String instrumentationKey;

	private readonly JsonSerializerOptions serializerOptionsWithEnumConverter;

	private readonly TelemetryFactory telemetryFactory;

	#endregion

	#region Constructor

	public JsonTelemetrySerializerTests()
	{
		instrumentationKey = Guid.NewGuid().ToString();

		var converter = new JsonStringEnumConverter();

		serializerOptionsWithEnumConverter = new JsonSerializerOptions();

		serializerOptionsWithEnumConverter.Converters.Add(converter);

		telemetryFactory = new()
		{
			Measurements = [new("m", 0), new("n", 1.5), new("k", -0.1)],

			Properties = [new("source", "tests")],

			Tags =
			[
				new(TelemetryTagKeys.CloudRole, "TestMachine"),
				new(TelemetryTagKeys.OperationName, nameof(JsonTelemetrySerializerTests)),
				new(TelemetryTagKeys.OperationId, TelemetryFactory.GetOperationId()),
			]
		};
	}

	#endregion

	#region Methods: Tests - All Telemetry Types

	[TestMethod]
	public void Method_Serialize_AvailabilityTelemetry()
	{
		const String expectedName = @"AppAvailabilityResults";
		const String expectedType = @"AvailabilityData";

		// arrange
		var telemetry = telemetryFactory.Create_AvailabilityTelemetry_Max("Check");

		// act
		var asString = Serialize(instrumentationKey, telemetry);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(telemetry, t => t.Duration, @"duration", dataElement);
		DeserializeAndAssert(telemetry, t => t.Id, @"id", dataElement);
		DeserializeAndAssert(telemetry, t => t.Measurements, @"measurements", dataElement);
		DeserializeAndAssert(telemetry, t => t.Message, @"message", dataElement);
		DeserializeAndAssert(telemetry, t => t.Name, @"name", dataElement);
		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", dataElement);
		DeserializeAndAssert(telemetry, t => t.RunLocation, @"runLocation", dataElement);
		DeserializeAndAssert(telemetry, t => t.Success, @"success", dataElement);
		DeserializeAndAssert(telemetry, t => t.Tags, @"tags", rootElement);
		DeserializeAndAssert(telemetry, t => t.Time, @"time", rootElement);
	}

	[TestMethod]
	public void Method_Serialize_DependencyTelemetry_Max()
	{
		const String expectedName = @"AppDependencies";
		const String expectedType = @"RemoteDependencyData";

		// arrange
		var telemetry = telemetryFactory.Create_DependencyTelemetry_Max("Storage", new Uri("https://dependency.sample.url"));

		// act
		var asString = Serialize(instrumentationKey, telemetry);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(telemetry, t => t.Data, @"data", dataElement);
		DeserializeAndAssert(telemetry, t => t.Duration, @"duration", dataElement);
		DeserializeAndAssert(telemetry, t => t.Id, @"id", dataElement);
		DeserializeAndAssert(telemetry, t => t.Measurements, @"measurements", dataElement);
		DeserializeAndAssert(telemetry, t => t.Name, @"name", dataElement);
		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", rootElement);
		DeserializeAndAssert(telemetry, t => t.ResultCode, @"resultCode", dataElement);
		DeserializeAndAssert(telemetry, t => t.Success, @"success", dataElement);
		DeserializeAndAssert(telemetry, t => t.Target, @"target", dataElement);
		DeserializeAndAssert(telemetry, t => t.Tags, @"tags", rootElement);
		DeserializeAndAssert(telemetry, t => t.Type, @"type", dataElement);
	}

	[TestMethod]
	public void Method_Serialize_DependencyTelemetry_Min()
	{
		const String expectedName = @"AppDependencies";
		const String expectedType = @"RemoteDependencyData";

		// arrange
		var telemetry = TelemetryFactory.Create_DependencyTelemetry_Min("Storage");

		// act
		var asString = Serialize(instrumentationKey, telemetry);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(telemetry, t => t.Data, @"data", dataElement);
		DeserializeAndAssert(telemetry, t => t.Duration, @"duration", dataElement);
		DeserializeAndAssert(telemetry, t => t.Id, @"id", dataElement);
		DeserializeAndAssert(telemetry, t => t.Measurements, @"measurements", dataElement);
		DeserializeAndAssert(telemetry, t => t.Name, @"name", dataElement);
		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", rootElement);
		DeserializeAndAssert(telemetry, t => t.ResultCode, @"resultCode", dataElement);
		DeserializeAndAssert(telemetry, t => t.Success, @"success", dataElement);
		DeserializeAndAssert(telemetry, t => t.Target, @"target", dataElement);
		DeserializeAndAssert(telemetry, t => t.Tags, @"tags", rootElement);
		DeserializeAndAssert(telemetry, t => t.Type, @"type", dataElement);
	}

	[TestMethod]
	public void Method_Serialize_EventTelemetry()
	{
		const String expectedName = @"AppEvents";
		const String expectedType = @"EventData";

		// arrange
		var telemetry = telemetryFactory.Create_EventTelemetry_Max("Check");

		// act
		var asString = Serialize(instrumentationKey, telemetry);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(telemetry, t => t.Measurements, @"measurements", dataElement);
		DeserializeAndAssert(telemetry, t => t.Name, @"name", dataElement);
		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", rootElement);
		DeserializeAndAssert(telemetry, t => t.Tags, @"tags", rootElement);
		DeserializeAndAssert(telemetry, t => t.Time, @"time", rootElement);
	}

	[TestMethod]
	public void Method_Serialize_ExceptionTelemetry()
	{
		const String expectedName = @"AppExceptions";
		const String expectedType = @"ExceptionData";

		// arrange
		var telemetry = telemetryFactory.Create_ExceptionTelemetry_Max();

		// act
		var asString = Serialize(instrumentationKey, telemetry);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(telemetry, t => t.Measurements, @"measurements", dataElement);
		DeserializeAndAssert(telemetry, t => t.ProblemId, @"problemId", dataElement);
		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", rootElement);
		DeserializeAndAssert(telemetry, t => t.SeverityLevel, @"severityLevel", dataElement, serializerOptionsWithEnumConverter);
		DeserializeAndAssert(telemetry, t => t.Tags, @"tags", rootElement);
		DeserializeAndAssert(telemetry, t => t.Time, @"time", rootElement);
	}

	[TestMethod]
	public void Method_Serialize_MetricTelemetry()
	{
		const String expectedName = @"AppMetrics";
		const String expectedType = @"MetricData";

		// arrange
		var aggregation = new MetricValueAggregation()
		{
			Count = 3,
			Min = 1,
			Max = 3
		};
		var telemetry = telemetryFactory.Create_MetricTelemetry_Max("tests", "count", 6, aggregation);

		// act
		var asString = Serialize(instrumentationKey, telemetry);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		var metricElement = dataElement.GetProperty(@"metrics")[0];

		DeserializeAndAssert(aggregation, t => t.Count, @"count", metricElement);
		DeserializeAndAssert(aggregation, t => t.Max, @"max", metricElement);
		DeserializeAndAssert(aggregation, t => t.Min, @"min", metricElement);
		DeserializeAndAssert(telemetry, t => t.Name, @"name", metricElement);
		DeserializeAndAssert(telemetry, t => t.Namespace, @"ns", metricElement);
		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", dataElement);
		DeserializeAndAssert(telemetry, t => t.Tags, @"tags", rootElement);
		DeserializeAndAssert(telemetry, t => t.Time, @"time", rootElement);
		DeserializeAndAssert(telemetry, t => t.Value, @"value", metricElement);
	}

	[TestMethod]
	public void Method_Serialize_PageViewTelemetry()
	{
		const String expectedName = @"AppPageViews";
		const String expectedType = @"PageViewData";

		// arrange
		var telemetry = telemetryFactory.Create_PageViewTelemetry_Max("Main", new Uri("https://page.sample.url"));

		// act
		var asString = Serialize(instrumentationKey, telemetry);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(telemetry, t => t.Duration, @"duration", dataElement);
		DeserializeAndAssert(telemetry, t => t.Id, @"id", dataElement);
		DeserializeAndAssert(telemetry, t => t.Measurements, @"measurements", dataElement);
		DeserializeAndAssert(telemetry, t => t.Name, @"name", dataElement);
		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", rootElement);
		DeserializeAndAssert(telemetry, t => t.Tags, @"tags", rootElement);
		DeserializeAndAssert(telemetry, t => t.Time, @"time", rootElement);
		DeserializeAndAssert(telemetry, t => t.Url, @"url", dataElement);
	}

	[TestMethod]
	public void Method_Serialize_RequestTelemetry_Min()
	{
		const String expectedName = @"AppRequests";
		const String expectedType = @"RequestData";

		// arrange
		var telemetryMax = telemetryFactory.Create_RequestTelemetry_Max("GetMain", new Uri("exe:test"));
		var telemetryMin = new RequestTelemetry
		{
			Id = TelemetryFactory.GetActivityId(),
			ResponseCode = "OK",
			Time = DateTime.UtcNow,
			Url = new Uri("exe:test")
		};
		RequestTelemetry[] telemetryList = [telemetryMax, telemetryMin];

		foreach (var telemetry in telemetryList)
		{
			// act
			var asString = Serialize(instrumentationKey, telemetry);

			// assert
			DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

			DeserializeAndAssert(telemetry, t => t.Duration, @"duration", dataElement);
			DeserializeAndAssert(telemetry, t => t.Id, @"id", dataElement);
			DeserializeAndAssert(telemetry, t => t.Measurements, @"measurements", dataElement);
			DeserializeAndAssert(telemetry, t => t.Name, @"name", dataElement);
			DeserializeAndAssert(telemetry, t => t.Properties, @"properties", rootElement);
			DeserializeAndAssert(telemetry, t => t.ResponseCode, @"responseCode", dataElement);
			DeserializeAndAssert(telemetry, t => t.Success, @"success", dataElement);
			DeserializeAndAssert(telemetry, t => t.Tags, @"tags", rootElement);
			DeserializeAndAssert(telemetry, t => t.Time, @"time", rootElement);
			DeserializeAndAssert(telemetry, t => t.Url, @"url", dataElement);
		}
	}

	[TestMethod]
	public void Method_Serialize_TraceTelemetry()
	{
		// arrange
		const String expectedName = @"AppTraces";
		const String expectedType = @"MessageData";

		var telemetry = telemetryFactory.Create_TraceTelemetry_Max("Test");

		// act
		var asString = Serialize(instrumentationKey, telemetry);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(telemetry, t => t.Message, @"message", dataElement);
		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", rootElement);
		DeserializeAndAssert(telemetry, t => t.SeverityLevel, @"severityLevel", dataElement, serializerOptionsWithEnumConverter);
		DeserializeAndAssert(telemetry, t => t.Tags, @"tags", rootElement);
		DeserializeAndAssert(telemetry, t => t.Time, @"time", rootElement);
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
			JsonTelemetrySerializer.Serialize(streamWriter, instrumentationKey, telemetry);
		}

		// assert
		Assert.AreEqual(3, memoryStream.Position);
	}

	#endregion

	#region Methods: Tests - Extra Cases

	/// <summary>
	/// Extra test for scenario when Telemetry has null collection properties.
	/// </summary>
	[TestMethod]
	public void Method_Serialize_Telemetry_With_CollectionProperties_Are_Null()
	{
		const String expectedName = @"AppEvents";
		const String expectedType = @"EventData";

		// arrange
		var telemetry = new EventTelemetry
		{
			Name = "Test",
			Time = DateTime.UtcNow
		};

		// act
		var asString = Serialize(instrumentationKey, telemetry);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(telemetry, t => t.Measurements, @"measurements", dataElement);
		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", rootElement);
		DeserializeAndAssert(telemetry, t => t.Tags, @"tags", rootElement);
	}

	/// <summary>
	/// Extra test for scenario when Telemetry has empty properties.
	/// </summary>
	[TestMethod]
	public void Method_Serialize_Telemetry_With_CollectionProperties_Are_Empty()
	{
		const String expectedName = @"AppEvents";
		const String expectedType = @"EventData";

		// arrange
		var telemetry = new EventTelemetry
		{
			Measurements = [],
			Name = "Test",
			Properties = [],
			Tags = [],
			Time = DateTime.UtcNow
		};

		var expectedTelemetry = new EventTelemetry
		{
			Name = "Test",
			Time = DateTime.UtcNow
		};

		// act
		var asString = Serialize(instrumentationKey, telemetry);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(expectedTelemetry, t => t.Measurements, @"measurements", dataElement);
		DeserializeAndAssert(expectedTelemetry, t => t.Properties, @"properties", rootElement);
		DeserializeAndAssert(expectedTelemetry, t => t.Tags, @"tags", rootElement);
	}

	/// <summary>
	/// Extra test for scenario when Telemetry has empty properties.
	/// </summary>
	[TestMethod]
	public void Method_Serialize_Telemetry_With_CollectionProperties_ContainsEmptyOrNullValues()
	{
		const String expectedName = @"AppEvents";
		const String expectedType = @"EventData";

		// arrange
		var measurement = new KeyValuePair<String, Double>("power", 1.0);
		var name = "Test";
		var property = new KeyValuePair<String, String>("isGood", "false");
		var tag = new KeyValuePair<String, String>(TelemetryTagKeys.CloudRole, "Tester");
		var time = DateTime.UtcNow;

		var telemetry = new EventTelemetry
		{
			Measurements = [ new (null!, 1.0), new ("", 2.0), new("   ", 3.0) ],
			Name = name,
			Properties =
			[
				new (null!, "test"), new ("", "test"), new("   ", "test"),
				new ( "test", null!), new ( "test",""), new("test","   ")
			],
			Tags =
			[
				new (null!, "test"), new ("", "test"), new("   ", "test"),
				new ( "test", null!), new ( "test",""), new("test","   ")
			],
			Time = time
		};

		var expectedTelemetry = new EventTelemetry
		{
			Measurements = [],
			Name = name,
			Properties = [],
			Tags = [],
			Time = time
		};

		// act
		var asString = Serialize(instrumentationKey, telemetry);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(expectedTelemetry, t => t.Measurements, @"measurements", dataElement);
		DeserializeAndAssert(expectedTelemetry, t => t.Properties, @"properties", rootElement);
		DeserializeAndAssert(expectedTelemetry, t => t.Tags, @"tags", rootElement);
	}

	#endregion

	#region Methods: Helpers

	/// <summary>
	/// Serializes the telemetry object to string.
	/// </summary>
	private static String Serialize
	(
		String instrumentationKey,
		Telemetry telemetry
	)
	{
		var memoryStream = new MemoryStream();

		using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 32768, true))
		{
			JsonTelemetrySerializer.Serialize(streamWriter, instrumentationKey, telemetry);
		}

		memoryStream.Position = 0;

		using var streamReader = new StreamReader(memoryStream);

		var result = streamReader.ReadToEnd();

		return result;
	}

	/// <summary>
	/// Deserializes the telemetry object from string and asserts to the expected values.
	/// </summary>
	private static void DeserializeAndAssert
	(
		String streamAsString,
		String expectedInstrumentationKey,
		String expectedName,
		String expectedType,
		out JsonElement rootElement,
		out JsonElement dataElement
	)
	{
		var document = JsonSerializer.Deserialize<JsonDocument>(streamAsString);

		Assert.IsNotNull(document);

		rootElement = document.RootElement;

		DeserializeAndAssert(expectedInstrumentationKey, @"iKey", rootElement);

		DeserializeAndAssert(expectedName, @"name", rootElement);

		var childElement = rootElement.GetProperty(@"data");

		DeserializeAndAssert(expectedType, @"baseType", childElement);

		dataElement = childElement.GetProperty(@"baseData");
	}

	/// <summary>
	/// Deserializes the value of the property of the JSON element and asserts to the expected value.
	/// </summary>
	private static void DeserializeAndAssert<ElementType>
	(
		ElementType? expectedValue,
		String jsonPropertyName,
		JsonElement jsonElement,
		JsonSerializerOptions? options = null
	)
	{
		var actualValue = jsonElement.GetProperty(jsonPropertyName).Deserialize<ElementType>(options);

		Assert.AreEqual(expectedValue, actualValue, jsonPropertyName);
	}

	/// <summary>
	/// Deserializes the value of the property of the JSON element and asserts to the expected value.
	/// </summary>
	private static void DeserializeAndAssert<ObjectType, PropertyType>
	(
		ObjectType expectedObject,
		Expression<Func<ObjectType, PropertyType>> propertySelector,
		String jsonPropertyName,
		JsonElement jsonElement,
		JsonSerializerOptions? options = null
	)
	{
		PropertyType? actualValue = default;

		if (jsonElement.TryGetProperty(jsonPropertyName, out var jsonProperty))
		{
			actualValue = jsonProperty.Deserialize<PropertyType>(options);
		}

		AssertHelper.GetPropertyValueAndMetadata(expectedObject, propertySelector, out var expectedValue, out var typeName, out var propertyName);

		Assert.AreEqual(expectedValue, actualValue, "{0}.{1}", typeName, propertyName);
	}

	/// <summary>
	/// Deserializes the value of the property of the JSON element and asserts to the expected value.
	/// </summary>
	private static void DeserializeAndAssert<ObjectType, KeyType, ValueType>
	(
		ObjectType expectedObject,
		Expression<Func<ObjectType, IReadOnlyList<KeyValuePair<KeyType, ValueType>>?>> propertySelector,
		String jsonPropertyName,
		JsonElement jsonElement
	)
		where KeyType : notnull
	{
		Dictionary<KeyType, ValueType>? actualValue = null;

		if (jsonElement.TryGetProperty(jsonPropertyName, out var jsonProperty))
		{
			actualValue = jsonProperty.Deserialize<Dictionary<KeyType, ValueType>>();
		}

		AssertHelper.GetPropertyValueAndMetadata(expectedObject, propertySelector, out var expectedValue, out var typeName, out var propertyName);

		var comparer = new KeyValuePairEqualityComparer<KeyType, ValueType>
		(
			EqualityComparer<KeyType>.Default,
			EqualityComparer<ValueType>.Default
		);

		CollectionAssert.AreEquivalent(expectedValue, actualValue, comparer, "{0}.{1}", typeName, propertyName);
	}

	#endregion
}
