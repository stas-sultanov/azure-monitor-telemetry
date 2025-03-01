// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.IntegrationTests;

using Azure.Monitor.Telemetry.Tests;

/// <summary>
/// Set of integration tests for all types of telemetry that implements <see cref="Telemetry"/>.
/// The goal is to ensure that all telemetry types can be tracked and published successfully.
/// </summary>
[TestCategory("IntegrationTests")]
[TestClass]
public sealed class TelemetryTypesTests : IntegrationTestsBase
{
	#region Fields

	private readonly TelemetryFactory telemetryFactory = new();

	private TelemetryTracker TelemetryTracker { get; }

	#endregion

	#region Constructor

	/// <param name="testContext">The test context.</param>
	public TelemetryTypesTests(TestContext testContext) : base(
		testContext,
		new PublisherConfiguration()
		{
			ConfigPrefx = @"Azure.Monitor.AuthOff.",
			UseAuthentication = false
		}
	)
	{
		TelemetryTracker = new TelemetryTracker
		(
			TelemetryPublishers,
			[
				new (TelemetryTagKey.CloudRole, "Tester"),
				new (TelemetryTagKey.CloudRoleInstance, Environment.MachineName)
			]
		);
	}

	#endregion

	#region Tests: AvailabilityTelemetry

	/// <summary>
	/// Tests <see cref="AvailabilityTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_AvailabilityTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_AvailabilityTelemetry_Max();

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

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
		var telemetry = telemetryFactory.Create_AvailabilityTelemetry_Min();

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Tests: DependencyTelemetry

	/// <summary>
	/// Tests <see cref="DependencyTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_DependencyTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_DependencyTelemetry_Max();

		// act
		TelemetryTracker.Add(telemetry);
		var publishResult = await TelemetryTracker.PublishAsync();

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
		var telemetry = telemetryFactory.Create_DependencyTelemetry_Min();

		// act
		TelemetryTracker.Add(telemetry);
		var publishResult = await TelemetryTracker.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Tests: EventTelemetry

	/// <summary>
	/// Tests <see cref="EventTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_EventTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_EventTelemetry_Max();

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

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
		var telemetry = telemetryFactory.Create_EventTelemetry_Min();

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Tests: ExceptionTelemetry

	/// <summary>
	/// Tests <see cref="ExceptionTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_ExceptionTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_ExceptionTelemetry_Max();

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

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
		var telemetry = telemetryFactory.Create_ExceptionTelemetry_Min();

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Tests: MetricTelemetry

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
		var telemetry = telemetryFactory.Create_MetricTelemetry_Max("tests", 6, aggregation);

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

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
		var telemetry = telemetryFactory.Create_MetricTelemetry_Min("tests", 6);

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Tests: PageViewTelemetry

	/// <summary>
	/// Tests <see cref="PageViewTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_PageViewTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_PageViewTelemetry_Max();

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

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
		var telemetry = telemetryFactory.Create_PageViewTelemetry_Min();

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Tests: RequestTelemetry

	/// <summary>
	/// Tests <see cref="RequestTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_RequestTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_RequestTelemetry_Max();

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

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
		var telemetry = telemetryFactory.Create_PageViewTelemetry_Min();

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion

	#region Tests: TraceTelemetry

	/// <summary>
	/// Tests <see cref="TraceTelemetry"/> with full load.
	/// </summary>
	[TestMethod]
	public async Task Type_TraceTelemetry_Max()
	{
		// arrange
		var telemetry = telemetryFactory.Create_TraceTelemetry_Max();

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

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
		var telemetry = telemetryFactory.Create_TraceTelemetry_Min();

		// act
		TelemetryTracker.Add(telemetry);

		var publishResult = await TelemetryTracker.PublishAsync();

		// assert
		AssertStandardSuccess(publishResult);
	}

	#endregion
}
