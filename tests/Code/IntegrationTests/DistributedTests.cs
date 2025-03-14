﻿// Created by Stas Sultanov.
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

	private TelemetryClient ClientTelemetryClient { get; }
	private TelemetryClient Service0TelemetryClient { get; }
	private TelemetryClient Service1TelemetryClient { get; }

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
		ClientTelemetryClient = new TelemetryClient
		(
			TelemetryPublishers,
			[
				new(TelemetryTagKeys.CloudRole, "Frontend"),
				new(TelemetryTagKeys.CloudRoleInstance, Random.Shared.Next(0,100).ToString(CultureInfo.InvariantCulture)),
				new(TelemetryTagKeys.DeviceType, "Browser"),
				new(TelemetryTagKeys.LocationIp, clientIP)
			]
		);

		clientTelemetryTrackedHttpClientHandler = new TelemetryTrackedHttpClientHandler(ClientTelemetryClient, TelemetryFactory.GetActivityId);

		Service0TelemetryClient = new TelemetryClient
		(
			TelemetryPublishers,
			[
				new(TelemetryTagKeys.CloudRole, "Watchman"),
				new(TelemetryTagKeys.CloudRoleInstance, Random.Shared.Next(100,200).ToString(CultureInfo.InvariantCulture)),
				new(TelemetryTagKeys.LocationIp, service0IP)
			]
		);

		Service1TelemetryClient = new TelemetryClient
		(
			TelemetryPublishers,
			[
				new(TelemetryTagKeys.CloudRole, "Backend"),
				new(TelemetryTagKeys.CloudRoleInstance, Random.Shared.Next(200,300).ToString(CultureInfo.InvariantCulture)),
				new(TelemetryTagKeys.LocationIp, service1IP)
			]
		);

		service1TelemetryTrackedHttpClientHandler = new TelemetryTrackedHttpClientHandler(Service1TelemetryClient, TelemetryFactory.GetActivityId);
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
			ClientTelemetryClient.Operation = new TelemetryOperation
			{
				Id = TelemetryFactory.GetOperationId(),
				Name = "ShowMainPage"
			};

			// simulate top level operation - page view
			await TelemetrySimulator.SimulatePageViewAsync
			(
				ClientTelemetryClient,
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
						ClientTelemetryClient,
						HttpMethod.Get,
						requestUrl,
						HttpStatusCode.OK,
						(cancellationToken) => Service1ServeRequestAsync
						(
							ClientTelemetryClient.Operation,
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
			Service0TelemetryClient.Operation = new TelemetryOperation
			{
				Id = TelemetryFactory.GetOperationId(),
				Name = "Availability"
			};

			// simulate top level operation - availability test
			await TelemetrySimulator.SimulateAvailabilityAsync
			(
				Service0TelemetryClient,
				"Check Health",
				"Passed",
				true,
				"West Europe",
				(cancellationToken) => Service1ServeRequestAsync
				(
					Service0TelemetryClient.Operation,
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
		var clientPublishResult = await ClientTelemetryClient.PublishAsync(cancellationToken);

		// publish server telemetry
		var service0PublishResult = await Service0TelemetryClient.PublishAsync(cancellationToken);

		// publish server telemetry
		var service1PublishResult = await Service1TelemetryClient.PublishAsync(cancellationToken);

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
		Service1TelemetryClient.TrackTrace("Request from Main Page", SeverityLevel.Information);
	}

	private async Task Service1ServeAvailabilityRequestInternalAsync(CancellationToken cancellationToken)
	{
		// simulate execution delay
		await Task.Delay(Random.Shared.Next(100), cancellationToken);

		// add Trace
		Service1TelemetryClient.TrackTrace("Health Request", SeverityLevel.Information);
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
		Service1TelemetryClient.Operation = operation;

		await TelemetrySimulator.SimulateRequestAsync(Service1TelemetryClient, url, responseCode, success, subsequent, cancellationToken);
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
