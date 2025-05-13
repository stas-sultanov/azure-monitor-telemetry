// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.TelemetryTests;

using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Models;

/// <summary>
/// Set of integration tests for all types of telemetry that implements <see cref="Telemetry"/>.
/// The goal is to ensure that all telemetry types can be tracked and published successfully.
/// </summary>
[TestCategory("IntegrationTests")]
[TestClass]
public sealed class TelemetryTypesTests : IntegrationTestsBase
{
	#region Fields

	private readonly Uri defaultUri = new ("https://gostas.dev");

	private readonly TelemetryFactory telemetryFactory;

	private readonly TelemetryClient telemetryClient;

	#endregion

	#region Constructors

	/// <param name="testContext">The test context.</param>
	public TelemetryTypesTests
	(
		in TestContext testContext
	)
		: base
	(
		testContext,
		new PublisherConfiguration()
		{
			ConfigPrefix = "Azure.Monitor.AuthOff.",
			Authenticate = false
		}
	)
	{
		telemetryClient = new TelemetryClient(TelemetryPublishers)
		{
			Context = new()
			{
				CloudRole = "TestMachine",
				CloudRoleInstance = Environment.MachineName
			}
		};

		telemetryFactory = new()
		{
			Tags =
			[
				new(TelemetryTagKeys.OperationName, nameof(TelemetryTypesTests)),
				new(TelemetryTagKeys.OperationId, TelemetryFactory.GetOperationId()),
			]
		};
	}

	#endregion

	#region Methods: Tests - AvailabilityTelemetry

	/// <summary>
	/// Tests <see cref="AvailabilityTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_AvailabilityTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_AvailabilityTelemetry_Max("Check");

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	/// <summary>
	/// Tests <see cref="AvailabilityTelemetry"/> with minimum load.
	/// </summary>
	[TestMethod]
	public async Task Type_AvailabilityTelemetry_Min()
	{
		// arrange
		var telemetry = TelemetryFactory.Create_AvailabilityTelemetry_Min("Check");

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Methods: Tests - DependencyTelemetry

	/// <summary>
	/// Tests <see cref="DependencyTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_DependencyTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_DependencyTelemetry_Max("Storage", defaultUri);

		// act
		telemetryClient.Add(telemetry);
		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	/// <summary>
	/// Tests <see cref="DependencyTelemetry"/> with minimum load.
	/// </summary>
	[TestMethod]
	public async Task Type_DependencyTelemetry_Min()
	{
		// arrange
		var telemetry = TelemetryFactory.Create_DependencyTelemetry_Min("Storage");

		// act
		telemetryClient.Add(telemetry);
		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Methods: Tests - EventTelemetry

	/// <summary>
	/// Tests <see cref="EventTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_EventTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_EventTelemetry_Max("Check");

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	/// <summary>
	/// Tests <see cref="EventTelemetry"/> with minimum load.
	/// </summary>
	[TestMethod]
	public async Task Type_EventTelemetry_Min()
	{
		// arrange
		var telemetry = TelemetryFactory.Create_EventTelemetry_Min("Check");

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Methods: Tests - ExceptionTelemetry

	/// <summary>
	/// Tests <see cref="ExceptionTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_ExceptionTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_ExceptionTelemetry_Max(Guid.NewGuid().ToString(), SeverityLevel.Critical);

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	/// <summary>
	/// Tests <see cref="ExceptionTelemetry"/> with minimum load.
	/// </summary>
	[TestMethod]
	public async Task Type_ExceptionTelemetry_Min()
	{
		// arrange
		var telemetry = TelemetryFactory.Create_ExceptionTelemetry_Min();

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Methods: Tests - MetricTelemetry

	/// <summary>
	/// Tests <see cref="MetricTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_MetricTelemetry_Max()
	{
		// arrange
		var aggregation = new MetricValueAggregation()
		{
			Count = 3,
			Min = 1,
			Max = 3
		};
		var telemetry = telemetryFactory.Create_MetricTelemetry_Max("tests", "count", 6, aggregation);

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	/// <summary>
	/// Tests <see cref="MetricTelemetry"/> with minimum load.
	/// </summary>
	[TestMethod]
	public async Task Type_MetricTelemetry_Min()
	{
		// arrange
		var telemetry = TelemetryFactory.Create_MetricTelemetry_Min("tests", "count", 6);

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Methods: Tests - PageViewTelemetry

	/// <summary>
	/// Tests <see cref="PageViewTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_PageViewTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_PageViewTelemetry_Max("Main", defaultUri);

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	/// <summary>
	/// Tests <see cref="PageViewTelemetry"/> with minimum load.
	/// </summary>
	[TestMethod]
	public async Task Type_PageViewTelemetry_Min()
	{
		// arrange
		var telemetry = TelemetryFactory.Create_PageViewTelemetry_Min("Main");

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Methods: Tests - RequestTelemetry

	/// <summary>
	/// Tests <see cref="RequestTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_RequestTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_RequestTelemetry_Max("GetMain", defaultUri);

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	/// <summary>
	/// Tests <see cref="RequestTelemetry"/> with minimum load.
	/// </summary>
	[TestMethod]
	public async Task Type_RequestTelemetry_Min()
	{
		// arrange
		var telemetry = TelemetryFactory.Create_RequestTelemetry_Min("OK", defaultUri);

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Methods: Tests - TraceTelemetry

	/// <summary>
	/// Tests <see cref="TraceTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_TraceTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_TraceTelemetry_Max("Test");

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	/// <summary>
	/// Tests <see cref="TraceTelemetry"/> with minimum load.
	/// </summary>
	[TestMethod]
	public async Task Type_TraceTelemetry_Min()
	{
		// arrange
		var telemetry = TelemetryFactory.Create_TraceTelemetry_Min("Test");

		// act
		telemetryClient.Add(telemetry);

		var publishResult = await telemetryClient.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion
}
