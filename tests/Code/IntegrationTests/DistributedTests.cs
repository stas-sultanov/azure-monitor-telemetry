// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.IntegrationTests;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;

using Azure.Monitor.Telemetry.Dependency;
using Azure.Monitor.Telemetry.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// The goals of this test:
/// - publish telemetry data into two instances of AppInsights; one with auth, one without auth.
/// - test dependency tracking with <see cref="TelemetryTrackedHttpClientHandler"/>.
/// </summary>
[TestCategory("IntegrationTests")]
[TestClass]
public sealed class DistributedTests : IntegrationTestsBase
{
	#region Data

	private const String clientIP = "78.26.233.104"; // Ukraine / Odessa
	private static readonly String service0IP = $"4.210.128.{Random.Shared.Next(1, 8)}";   // Azure DC
	private static readonly String service1IP = $"4.210.128.{Random.Shared.Next(16, 32)}"; // Azure DC

	private readonly TelemetryTrackedHttpClientHandler clientTelemetryTrackedHttpClientHandler;
	private readonly TelemetryTrackedHttpClientHandler service1TelemetryTrackedHttpClientHandler;

	#endregion

	private TelemetryTracker ClientTelemetryTracker { get; }
	private TelemetryTracker Service0TelemetryTracker { get; }
	private TelemetryTracker Service1TelemetryTracker { get; }

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="DependencyTrackingTests"/> class.
	/// </summary>
	/// <param name="testContext">The test context.</param>
	public DistributedTests(TestContext testContext)
		: base
		(
			testContext,
			new PublisherConfiguration()
			{
				ConfigPrefix = @"Azure.Monitor.AuthOn.",
				UseAuthentication = true
			}
		)
	{
		ClientTelemetryTracker = new TelemetryTracker
		(
			TelemetryPublishers,
			[
				new(TelemetryTagKey.CloudRole, "Frontend"),
				new(TelemetryTagKey.CloudRoleInstance, Random.Shared.Next(0,100).ToString(CultureInfo.InvariantCulture)),
				new(TelemetryTagKey.DeviceType, "Browser"),
				new(TelemetryTagKey.LocationIp, clientIP)
			]
		);

		clientTelemetryTrackedHttpClientHandler = new TelemetryTrackedHttpClientHandler(ClientTelemetryTracker, TelemetryFactory.GetActivityId);

		Service0TelemetryTracker = new TelemetryTracker
		(
			TelemetryPublishers,
			[
				new(TelemetryTagKey.CloudRole, "Watchman"),
				new(TelemetryTagKey.CloudRoleInstance, Random.Shared.Next(100,200).ToString(CultureInfo.InvariantCulture)),
				new(TelemetryTagKey.LocationIp, service0IP)
			]
		);

		Service1TelemetryTracker = new TelemetryTracker
		(
			TelemetryPublishers,
			[
				new(TelemetryTagKey.CloudRole, "Backend"),
				new(TelemetryTagKey.CloudRoleInstance, Random.Shared.Next(200,300).ToString(CultureInfo.InvariantCulture)),
				new(TelemetryTagKey.LocationIp, service1IP)
			]
		);

		service1TelemetryTrackedHttpClientHandler = new TelemetryTrackedHttpClientHandler(Service1TelemetryTracker, TelemetryFactory.GetActivityId);
	}

	#endregion

	public override void Dispose()
	{
		base.Dispose();

		clientTelemetryTrackedHttpClientHandler.Dispose();

		service1TelemetryTrackedHttpClientHandler.Dispose();
	}

	#region Methods: Tests

	[TestMethod]
	public async Task FromPageViewToRequestToDependency()
	{
		var cancellationToken = TestContext.CancellationTokenSource.Token;

		// page view
		{
			// set top level operation
			ClientTelemetryTracker.Operation = new TelemetryOperation
			{
				Id = TelemetryFactory.GetOperationId(),
				Name = "ShowMainPage"
			};

			// simulate top level operation - page view
			await TelemetrySimulator.SimulatePageViewAsync
			(
				ClientTelemetryTracker,
				"Main",
				new Uri("https://gostas.dev"),
				async (cancellationToken) =>
				{
					// make dependency call
					_ = await MakeDependencyCallAsyc(clientTelemetryTrackedHttpClientHandler, new Uri("https://google.com"), cancellationToken);

					// simulate internal work
					await Task.Delay(Random.Shared.Next(50, 100), cancellationToken);

					// simulate dependency call to server
					var requestUrl = new Uri("https://gostas.dev/int.js");

					await TelemetrySimulator.SimulateDependencyAsync
					(
						ClientTelemetryTracker,
						HttpMethod.Get,
						requestUrl,
						HttpStatusCode.OK,
						(cancellationToken) => Service1ServeRequestAsync
						(
							ClientTelemetryTracker.Operation,
							requestUrl,
							"OK",
							true,
							Service1ServePageViewRequestInternalAsync,
							cancellationToken
						),
						cancellationToken
					);
				},
				cancellationToken
			);
		}

		// availability test
		{
			// set top level operation
			Service0TelemetryTracker.Operation = new TelemetryOperation
			{
				Id = TelemetryFactory.GetOperationId(),
				Name = "Availability"
			};

			// simulate top level operation - availability test
			await TelemetrySimulator.SimulateAvailabilityAsync
			(
				Service0TelemetryTracker,
				"Check Health",
				"Passed",
				true,
				"West Europe",
				(cancellationToken) => Service1ServeRequestAsync
				(
					Service0TelemetryTracker.Operation,
					new Uri("https://gostas.dev/health"),
					"OK",
					true,
					Service1ServeAvailabilityRequestInternalAsync,
					cancellationToken
				),
				cancellationToken
			);
		}

		// publish client telemetry
		var clientPublishResult = await ClientTelemetryTracker.PublishAsync(cancellationToken);

		// publish server telemetry
		var service0PublishResult = await Service0TelemetryTracker.PublishAsync(cancellationToken);

		// publish server telemetry
		var service1PublishResult = await Service1TelemetryTracker.PublishAsync(cancellationToken);

		AssertStandardSuccess(clientPublishResult);

		AssertStandardSuccess(service0PublishResult);

		AssertStandardSuccess(service1PublishResult);
	}

	#endregion

	#region Methods: Helpers

	private async Task Service1ServePageViewRequestInternalAsync(CancellationToken cancellationToken)
	{
		// make dependency call
		_ = await MakeDependencyCallAsyc(service1TelemetryTrackedHttpClientHandler, new Uri("https://bing.com"), cancellationToken);

		// simulate execution delay
		await Task.Delay(Random.Shared.Next(100), cancellationToken);

		// add Trace
		Service1TelemetryTracker.TrackTrace("Request from Main Page", SeverityLevel.Information);
	}

	private async Task Service1ServeAvailabilityRequestInternalAsync(CancellationToken cancellationToken)
	{
		// simulate execution delay
		await Task.Delay(Random.Shared.Next(100), cancellationToken);

		// add Trace
		Service1TelemetryTracker.TrackTrace("Health Request", SeverityLevel.Information);
	}

	private async Task Service1ServeRequestAsync
	(
		TelemetryOperation operation,
		Uri url,
		String responseCode,
		Boolean success,
		Func<CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// set top level operation
		Service1TelemetryTracker.Operation = operation;

		await TelemetrySimulator.SimulateRequestAsync(Service1TelemetryTracker, url, responseCode, success, subsequent, cancellationToken);
	}

	public static async Task<String> MakeDependencyCallAsyc
(
	HttpMessageHandler messageHandler,
	Uri uri,
	CancellationToken cancellationToken
)
	{
		using var httpClient = new HttpClient(messageHandler, false);

		using var httpResponse = await httpClient.GetAsync(uri, cancellationToken);

		var result = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

		return result;
	}

	#endregion
}
