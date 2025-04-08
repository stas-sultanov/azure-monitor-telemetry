// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Azure.Monitor.Telemetry.Models;
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

	private readonly KeyValuePair<String, String> [] clientTags =
	[
		new(TelemetryTagKeys.CloudRole, "TestMachine"),
	];

	private readonly String instrumentationKey = Guid.NewGuid().ToString();
	private readonly TelemetryFactory telemetryFactory = new(nameof(HttpTelemetryPublisherTests));

	#endregion

	#region Methods: Tests

	[TestMethod]
	public void Method_Serialize_AvailabilityTelemetry()
	{
		const String expectedName = @"AppAvailabilityResults";
		const String expectedType = @"AvailabilityData";

		// arrange
		var telemetry = telemetryFactory.Create_AvailabilityTelemetry_Max("Check");
		var expectedTags = GetTags(telemetry, clientTags);

		// act
		var asString = Serialize(instrumentationKey, telemetry, clientTags);

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

		//var tags = GetTags(rootElement);

		DeserializeAndAssert(telemetry, t => t.Time, @"time", rootElement);
	}

	[TestMethod]
	public void Method_Serialize_DependencyTelemetry()
	{
		const String expectedName = @"AppDependencies";
		const String expectedType = @"RemoteDependencyData";

		// arrange
		var telemetry = telemetryFactory.Create_DependencyTelemetry_Max("Storage", testUrl );
		var expectedTags = GetTags(telemetry, clientTags);

		// act
		var asString = Serialize(instrumentationKey, telemetry, clientTags);

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

		//var tags = GetTags(rootElement);

		DeserializeAndAssert(telemetry, t => t.Type, @"type", dataElement);
	}

	[TestMethod]
	public void Method_Serialize_EventTelemetry()
	{
		const String expectedName = @"AppEvents";
		const String expectedType = @"EventData";

		// arrange
		var telemetry = telemetryFactory.Create_EventTelemetry_Max("Check");
		var expectedTags = GetTags(telemetry, clientTags);

		// act
		var asString = Serialize(instrumentationKey, telemetry, clientTags);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(telemetry, t => t.Measurements, @"measurements", dataElement);

		DeserializeAndAssert(telemetry, t => t.Name, @"name", dataElement);

		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", rootElement);

		//var tags = GetTags(rootElement);

		DeserializeAndAssert(telemetry, t => t.Time, @"time", rootElement);
	}

	[TestMethod]
	public void Method_Serialize_ExceptionTelemetry()
	{
		const String expectedName = @"AppExceptions";
		const String expectedType = @"ExceptionData";

		// arrange
		var telemetry = telemetryFactory.Create_ExceptionTelemetry_Max();
		var expectedTags = GetTags(telemetry, clientTags);

		// act
		var asString = Serialize(instrumentationKey, telemetry, clientTags);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(telemetry, t => t.Measurements, @"measurements", dataElement);

		DeserializeAndAssert(telemetry, t => t.ProblemId, @"problemId", dataElement);

		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", rootElement);

		DeserializeAndAssert(telemetry, t => t.SeverityLevel, @"severityLevel", dataElement, serializerOptionsWithEnumConverter);

		//var tags = GetTags(rootElement);

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
		var expectedTags = GetTags(telemetry, clientTags);

		// act
		var asString = Serialize(instrumentationKey, telemetry, clientTags);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		var metricElement = dataElement.GetProperty(@"metrics")[0];

		var count = metricElement.GetProperty(@"count").Deserialize<Int32>();

		var max = metricElement.GetProperty(@"max").Deserialize<Double>();

		var min = metricElement.GetProperty(@"min").Deserialize<Double>();

		DeserializeAndAssert(telemetry, t => t.Name, @"name", metricElement);

		DeserializeAndAssert(telemetry, t => t.Namespace, @"ns", metricElement);

		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", dataElement);

		//var tags = GetTags(rootElement);

		DeserializeAndAssert(telemetry, t => t.Time, @"time", rootElement);

		DeserializeAndAssert(telemetry, t => t.Value, @"value", metricElement);
	}

	[TestMethod]
	public void Method_Serialize_PageViewTelemetry()
	{
		const String expectedName = @"AppPageViews";
		const String expectedType = @"PageViewData";

		// arrange
		var telemetry = telemetryFactory.Create_PageViewTelemetry_Max("Main", testUrl);
		var expectedTags = GetTags(telemetry, clientTags);

		// act
		var asString = Serialize(instrumentationKey, telemetry, clientTags);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(telemetry, t => t.Duration, @"duration", dataElement);

		DeserializeAndAssert(telemetry, t => t.Id, @"id", dataElement);

		DeserializeAndAssert(telemetry, t => t.Measurements, @"measurements", dataElement);

		DeserializeAndAssert(telemetry, t => t.Name, @"name", dataElement);

		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", rootElement);

		//var tags = GetTags(rootElement);

		DeserializeAndAssert(telemetry, t => t.Time, @"time", rootElement);

		DeserializeAndAssert(telemetry, t => t.Url, @"url", dataElement);
	}

	[TestMethod]
	public void Method_Serialize_RequestTelemetry()
	{
		const String expectedName = @"AppRequests";
		const String expectedType = @"RequestData";

		// arrange
		var telemetry = telemetryFactory.Create_RequestTelemetry_Max("GetMain", testUrl);
		var expectedTags = GetTags(telemetry, clientTags);

		// act
		var asString = Serialize(instrumentationKey, telemetry, clientTags);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(telemetry, t => t.Duration, @"duration", dataElement);

		DeserializeAndAssert(telemetry, t => t.Id, @"id", dataElement);

		DeserializeAndAssert(telemetry, t => t.Measurements, @"measurements", dataElement);

		DeserializeAndAssert(telemetry, t => t.Name, @"name", dataElement);

		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", rootElement);

		DeserializeAndAssert(telemetry, t => t.ResponseCode, @"responseCode", dataElement);

		DeserializeAndAssert(telemetry, t => t.Success, @"success", dataElement);

		//var tags = GetTags(rootElement);

		DeserializeAndAssert(telemetry, t => t.Time, @"time", rootElement);

		DeserializeAndAssert(telemetry, t => t.Url, @"url", dataElement);
	}

	[TestMethod]
	public void Method_Serialize_TraceTelemetry()
	{
		// arrange
		const String expectedName = @"AppTraces";
		const String expectedType = @"MessageData";

		var telemetry = telemetryFactory.Create_TraceTelemetry_Max("Test");
		var expectedTags = GetTags(telemetry, clientTags);

		// act
		var asString = Serialize(instrumentationKey, telemetry, clientTags);

		// assert
		DeserializeAndAssert(asString, instrumentationKey, expectedName, expectedType, out var rootElement, out var dataElement);

		DeserializeAndAssert(telemetry, t => t.Message, @"message", dataElement);

		DeserializeAndAssert(telemetry, t => t.Properties, @"properties", rootElement);

		DeserializeAndAssert(telemetry, t => t.SeverityLevel, @"severityLevel", dataElement, serializerOptionsWithEnumConverter);

		//var tags = GetTags(rootElement);

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
			JsonTelemetrySerializer.Serialize(streamWriter, instrumentationKey, telemetry, null);
		}

		// assert
		Assert.AreEqual(3, memoryStream.Position);
	}

	#endregion

	#region Methods: Helpers

	private static String Serialize
	(
		String instrumentationKey,
		Telemetry telemetry,
		KeyValuePair<String, String>[] tags
	)
	{
		var memoryStream = new MemoryStream();

		using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 32768, true))
		{
			JsonTelemetrySerializer.Serialize(streamWriter, instrumentationKey, telemetry, tags);
		}

		memoryStream.Position = 0;

		using var streamReader = new StreamReader(memoryStream);

		var result = streamReader.ReadToEnd();

		return result;
	}

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

		DeserializeAndAssert(rootElement, @"iKey", expectedInstrumentationKey);

		DeserializeAndAssert(rootElement, @"name", expectedName);

		var childElement = rootElement.GetProperty(@"data");

		DeserializeAndAssert(childElement, @"baseType", expectedType);

		dataElement = childElement.GetProperty(@"baseData");
	}

	private static void DeserializeAndAssert<ObjectType, PropertyType>
	(
		ObjectType expectedObject,
		Expression<Func<ObjectType, PropertyType>> propertySelector,
		String jsonPropertyName,
		JsonElement jsonElement,
		JsonSerializerOptions? options = null
	)
	{
		var actualValue = jsonElement.GetProperty(jsonPropertyName).Deserialize<PropertyType>(options);

		AssertHelper.GetPropertyMetadata(expectedObject, propertySelector, out var expectedValue, out var typeName, out var propertyName);

		Assert.AreEqual(expectedValue, actualValue, "{0}.{1}", typeName, propertyName);
	}

	private static void DeserializeAndAssert<ObjectType, KeyType, ValueType>
	(
		ObjectType expectedObject,
		Expression<Func<ObjectType, IReadOnlyList<KeyValuePair<KeyType, ValueType>>?>> propertySelector,
		String jsonPropertyName,
		JsonElement jsonElement
	)
		where KeyType : notnull
	{
		var jsonProperty = jsonElement.GetProperty(jsonPropertyName);

		var actualValue = jsonProperty.Deserialize<Dictionary<KeyType, ValueType>>();

		AssertHelper.GetPropertyMetadata(expectedObject, propertySelector, out var expectedValue, out var typeName, out var propertyName);

		var comparer = new KeyValuePairEqualityComparer<KeyType, ValueType>
		(
			EqualityComparer<KeyType>.Default,
			EqualityComparer<ValueType>.Default
		);

		CollectionAssert.AreEquivalent(expectedValue, actualValue, comparer, "{0}.{1}", typeName, propertyName);
	}

	private static KeyValuePair<String, String>[] GetTags
	(
		Telemetry telemetry,
		KeyValuePair<String, String>[] tags
	)
	{
		var result = new List<KeyValuePair<String, String>>();

		if (telemetry.Operation != null)
		{
			if (!String.IsNullOrWhiteSpace(telemetry.Operation.Id))
			{
				result.Add(new KeyValuePair<String, String>(TelemetryTagKeys.OperationId, telemetry.Operation.Id));
			}

			if (!String.IsNullOrWhiteSpace(telemetry.Operation.Name))
			{
				result.Add(new KeyValuePair<String, String>(TelemetryTagKeys.OperationName, telemetry.Operation.Name));
			}

			if (!String.IsNullOrWhiteSpace(telemetry.Operation.ParentId))
			{
				result.Add(new KeyValuePair<String, String>(TelemetryTagKeys.OperationParentId, telemetry.Operation.ParentId));
			}
		}

		result.AddRange(tags);

		if (telemetry.Tags != null)
		{
			result.AddRange(telemetry.Tags);
		}

		return [.. result];
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
}
