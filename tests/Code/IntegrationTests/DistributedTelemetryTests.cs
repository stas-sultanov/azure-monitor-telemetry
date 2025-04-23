// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.TelemetryTests;

using System;
using System.Globalization;
using System.Net.Http;

using Azure.Monitor.Telemetry;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// The goals of this test:
/// - test distributed dependency tracking with AvailabilityTest, PageView, Request and Dependency telemetry.
/// </summary>
[TestCategory("IntegrationTests")]
[TestClass]
public sealed class DistributedTelemetryTests : IntegrationTestsBase
{
	#region Constants

	private static readonly String client_IP = $"78.26.233.{Random.Shared.Next(100, 200)}"; // Ukraine / Odessa
	private static readonly String probe_IP = $"4.210.128.{Random.Shared.Next(1, 8)}"; // Azure DC
	private static readonly String service_A_IP = $"4.210.128.{Random.Shared.Next(16, 32)}"; // Azure DC
	private static readonly String service_B_IP = $"4.210.128.{Random.Shared.Next(64, 128)}"; // Azure DC
	private static readonly String service_A_Domain = "a.services.gostas.dev";
	private static readonly String service_B_Domain = "b.services.gostas.dev";

	#endregion

	#region Fields

	private readonly TelemetryTags client_ContextTags;
	private readonly TelemetryTags probe_ContextTags;
	private readonly TelemetryTags service_A_ContextTags;
	private readonly TelemetryTags service_B_ContextTags;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="DependencyTrackingTests"/> class.
	/// </summary>
	/// <param name="testContext">The test context.</param>
	public DistributedTelemetryTests(TestContext testContext)
		: base
		(
			testContext,
			new PublisherConfiguration()
			{
				ConfigPrefix = "Azure.Monitor.AuthOn.",
				Authenticate = true
			}
		)
	{
		client_ContextTags = new()
		{
			CloudRole = "WebApp",
			CloudRoleInstance = Random.Shared.Next(0, 100).ToString(CultureInfo.InvariantCulture),
			DeviceType = "Browser",
			LocationIp = client_IP
		};

		probe_ContextTags = new()
		{
			CloudRole = "Probe",
			CloudRoleInstance = Random.Shared.Next(0, 100).ToString(CultureInfo.InvariantCulture),
			LocationIp = probe_IP
		};

		service_A_ContextTags = new()
		{
			CloudRole = "Service A",
			CloudRoleInstance = Random.Shared.Next(100, 200).ToString(CultureInfo.InvariantCulture),
			LocationIp = service_A_IP
		};

		service_B_ContextTags = new()
		{
			CloudRole = "Service B",
			CloudRoleInstance = Random.Shared.Next(200, 300).ToString(CultureInfo.InvariantCulture),
			LocationIp = service_B_IP
		};
	}

	#endregion

	#region Methods: Overrides of the Base Class

	/// <inheritdoc/>
	public override void Dispose()
	{
		base.Dispose();
	}

	#endregion

	#region Methods: Tests

	/// <summary>
	/// The scenario:
	/// [PROBE] ── AvailabilityTest ──> [SERVICE A]
	/// [PROBE] ── AvailabilityTest ──> [SERVICE B]
	/// </summary>
	[TestMethod]
	public async Task Probe_AvailabilityTest_To_ServiceA_And_ServiceB()
	{
		var cancellationToken = TestContext.CancellationTokenSource.Token;

		var operationId = TelemetryFactory.GetOperationId();
		var operationName = "HealthCheck";

		// create telemetry client for probe
		var probeTelemetryClient = new TelemetryClient(TelemetryPublishers)
		{
			Context = probe_ContextTags with
			{
				OperationId = operationId,
				OperationName = operationName
			}
		};

		// probe makes call to service A
		await TelemetrySimulator.SimulateAvailabilityTestCallAsync
		(
			probeTelemetryClient,
			"Service A",
			"West Europe",
			// service A accepts the call with request processing
			(parentActivityId, cancellationToken) => Service_ProcessRequest
			(
				service_A_ContextTags with
				{
					OperationId = operationId,
					OperationName = "HealthCheck",
					OperationParentId = parentActivityId
				},
				new Uri($"https://{service_A_Domain}/health"),
				// service A processes the request
				async (_, cancellationToken) =>
				{
					await Task.Delay(100, cancellationToken);
					return true;
				},
				cancellationToken
			),
			cancellationToken
		);

		// probe makes call to service B
		await TelemetrySimulator.SimulateAvailabilityTestCallAsync
		(
			probeTelemetryClient,
			"Service B",
			"West Europe",
			// service A accepts the call with request processing
			(parentActivityId, cancellationToken) => Service_ProcessRequest
			(
				service_B_ContextTags with
				{
					OperationId = operationId,
					OperationName = "HealthCheck",
					OperationParentId = parentActivityId
				},
				new Uri($"https://{service_B_Domain}/health"),
				// service A processes the request
				async (_, cancellationToken) =>
				{
					await Task.Delay(100, cancellationToken);
					return true;
				},
				cancellationToken
			),
			cancellationToken
		);

		var publishResults = await probeTelemetryClient.PublishAsync(cancellationToken);

		AssertStandardSuccess(publishResults);
	}

	/// <summary>
	/// The scenario:
	/// [Client] ──> {Main PageView} ── HTTP Request ──> [External]
	///                              └─ HTTP Request ──> [SERVICE A]
	/// </summary>
	[TestMethod]
	public async Task Client_PageView_To_External_And_Service_A()
	{
		var cancellationToken = TestContext.CancellationTokenSource.Token;

		var operationId = TelemetryFactory.GetOperationId();
		var operationName = "ShowMain";
		var service_A_RequestUrl = new Uri($"https://{service_A_Domain}/data");

		// create telemetry client for client
		var clientTelemetryClient = new TelemetryClient(TelemetryPublishers)
		{
			Context = client_ContextTags with
			{
				OperationId = operationId,
				OperationName = operationName
			}
		};

		// client makes page view
		await TelemetrySimulator.SimulatePageViewAsync
		(
			clientTelemetryClient,
			"Main",
			new Uri("https://www.gostas.dev"),
			// client makes dependency calls within the page view
			async (parentActivityId, cancellationToken) =>
			{
				// call external dependency
				await TelemetrySimulator.SimulateHttpDependencyCallAsync
				(
					clientTelemetryClient,
					HttpMethod.Get,
					new Uri("https://unpkg.com/vue@3/dist/vue.global.js"),
					async (_, cancellationToken) =>
					{
						await Task.Delay(20, cancellationToken);
						return true;
					},
					cancellationToken
				);

				// call service A
				await TelemetrySimulator.SimulateHttpDependencyCallAsync
				(
					clientTelemetryClient,
					HttpMethod.Get,
					service_A_RequestUrl,
					// service A accepts the call with request processing
					async (parentActivityId, cancellationToken) => await Service_ProcessRequest
					(
						service_A_ContextTags with
						{
							OperationId = operationId,
							OperationName = "GET DATA",
							OperationParentId = parentActivityId
						},
						service_A_RequestUrl,
						async (telemetryClient, cancellationToken) => await Service_Simulate_Dependency_SQL(telemetryClient, "SELECT * from [dbo].[Data]", cancellationToken),
						cancellationToken
					),
					cancellationToken
				);
			},
			cancellationToken
		);

		var publishResults = await clientTelemetryClient.PublishAsync(cancellationToken);

		AssertStandardSuccess(publishResults);
	}

	/// <summary>
	/// The scenario:
	/// [Client] ──> {Info PageView} ── HTTP Request ──> [SERVICE A] ── HTTP Request ──> [SERVICE B]
	/// </summary>
	[TestMethod]
	public async Task Client_PageView_To_Service_A_To_Service_B()
	{
		var cancellationToken = TestContext.CancellationTokenSource.Token;

		var operationId = TelemetryFactory.GetOperationId();
		var operationName = "ShowInfo";
		var service_A_RequestUrl = new Uri($"https://{service_A_Domain}/info");
		var service_B_RequestUrl = new Uri($"https://{service_B_Domain}/exrainfo");

		// create telemetry client for client
		var clientTelemetryClient = new TelemetryClient(TelemetryPublishers)
		{
			Context = client_ContextTags with
			{
				OperationId = operationId,
				OperationName = operationName
			}
		};

		// client makes page view
		await TelemetrySimulator.SimulatePageViewAsync
		(
			clientTelemetryClient,
			"Info",
			new Uri("https://www.gostas.dev/info"),
			// client makes dependency calls within the page view
			async (parentActivityId, cancellationToken) =>
			{
				// call service A
				await TelemetrySimulator.SimulateHttpDependencyCallAsync
				(
					clientTelemetryClient,
					HttpMethod.Get,
					service_A_RequestUrl,
					// service A accepts the call with request processing
					async (parentActivityId, cancellationToken) => await Service_ProcessRequest
					(
						service_A_ContextTags with
						{
							OperationId = operationId,
							OperationName = "GET INFO",
							OperationParentId = parentActivityId
						},
						service_A_RequestUrl,
						// service A makes dependency calls to service B
						async (serviceATelemetryClient, cancellationToken) =>
						{
							// call service A
							await TelemetrySimulator.SimulateHttpDependencyCallAsync
							(
								serviceATelemetryClient,
								HttpMethod.Get,
								service_B_RequestUrl,
								// service A accepts the call with request processing
								async (parentActivityId, cancellationToken) => await Service_ProcessRequest
								(
									service_B_ContextTags with
									{
										OperationId = operationId,
										OperationName = "GET INFO",
										OperationParentId = parentActivityId
									},
									service_B_RequestUrl,
									async (telemetryClient, cancellationToken) => await Service_Simulate_Dependency_SQL(telemetryClient, "SELECT * from [dbo].[Info]", cancellationToken),
									cancellationToken
								),
								cancellationToken
							);

							return true;
						},
						cancellationToken
					),
					cancellationToken
				);
			},
			cancellationToken
		);

		var publishResults = await clientTelemetryClient.PublishAsync(cancellationToken);

		AssertStandardSuccess(publishResults);
	}

	#endregion

	#region Methods: Helpers

	/// <summary>
	/// Simulate request processing.
	/// </summary>
	private async Task<Boolean> Service_ProcessRequest
	(
		TelemetryTags initialContext,
		Uri url,
		Func<TelemetryClient, CancellationToken, Task<Boolean>> subsequent,
		CancellationToken cancellationToken
	)
	{
		var telemetryClient = new TelemetryClient(TelemetryPublishers)
		{
			Context = initialContext
		};

		// begin activity scope
		telemetryClient.ActivityScopeBegin(TelemetryFactory.GetActivityId, out var time, out var timestamp, out var activityId, out var context);

		// execute subsequent
		var success = await subsequent(telemetryClient, cancellationToken);

		// end activity scope
		telemetryClient.ActivityScopeEnd(context, timestamp, out var duration);

		// track telemetry
		var responseCode = success ? "Success" : "Fail";

		telemetryClient.TrackRequest(time, duration, activityId, url, responseCode, success);

		// publish tracked telemetry
		var publishResults = await telemetryClient.PublishAsync(cancellationToken);

		AssertStandardSuccess(publishResults);

		// return result
		return success;
	}

	/// <summary>
	/// Simulate SQL dependency call.
	/// </summary>
	private static async Task<Boolean> Service_Simulate_Dependency_SQL
	(
		TelemetryClient telemetryClient,
		String command,
		CancellationToken cancellationToken
	)
	{
		var duration = TimeSpan.FromMilliseconds( Random.Shared.Next(200) );

		// simulate execution delay
		await Task.Delay(duration + TimeSpan.FromMilliseconds(Random.Shared.Next(50)), cancellationToken);

		// add Trace
		telemetryClient.TrackDependencySql
		(
			DateTime.UtcNow,
			duration,
			TelemetrySimulator.GetActivityId(),
			"test.database.windows.net",
			"db1",
			command,
			0
		);

		return true;
	}

	#endregion
}
