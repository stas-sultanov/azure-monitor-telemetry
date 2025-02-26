// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.IntegrationTests;
using System;
using System.Net;
using System.Net.Http;

using Azure.Monitor.Telemetry.Dependency;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// The goals of this test:
/// - publish telemetry data into two instances of AppInsights; one with auth, one without auth.
/// - test dependency tracking with <see cref="TelemetryTrackedHttpClientHandler"/>.
/// </summary>
[TestCategory("IntegrationTests")]
[TestClass]
public sealed class ClientToServerTests : AzureIntegrationTestsBase
{
	private const String QueueName = "commands";

	#region Data

	private readonly TelemetryTrackedHttpClientHandler clientTelemetryTrackedHttpClientHandler;

	private readonly TelemetryTrackedHttpClientHandler serverTelemetryTrackedHttpClientHandler;

	//private readonly KeyValuePair<String, String>[] testServerTags = [new(TelemetryTagKey.CloudRoleInstance, "Alpha"), new(TelemetryTagKey.LocationIp, "20.33.2.8")];

	#endregion

	private TelemetryTracker ClientTelemetryTracker { get; }

	private TelemetryTracker ServerTelemetryTracker { get; }

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="DependencyTrackingTests"/> class.
	/// </summary>
	/// <param name="testContext">The test context.</param>
	public ClientToServerTests(TestContext testContext)
		: base
		(
			testContext,
			[
				Tuple.Create(@"Azure.Monitor.AuthOn.", true, Array.Empty<KeyValuePair<String, String>>()),
			]
		)
	{
		ClientTelemetryTracker = new TelemetryTracker
		(
			TelemetryPublishers,
			[
				new(TelemetryTagKey.CloudRole, "Frontend"),
				new(TelemetryTagKey.CloudRoleInstance, Random.Shared.Next(100,200).ToString()),
				new(TelemetryTagKey.DeviceType, "Browser"),
				new(TelemetryTagKey.LocationIp, "78.26.233.104")
			]
		);

		clientTelemetryTrackedHttpClientHandler = new TelemetryTrackedHttpClientHandler(ClientTelemetryTracker, GetTelemetryId);

		ServerTelemetryTracker = new TelemetryTracker
		(
			TelemetryPublishers,
			[
				new(TelemetryTagKey.CloudRole, "Backend"),
				new(TelemetryTagKey.CloudRoleInstance, Environment.MachineName),
				new(TelemetryTagKey.LocationIp, "4.210.128.4")
			]
		);

		serverTelemetryTrackedHttpClientHandler = new TelemetryTrackedHttpClientHandler(ServerTelemetryTracker, GetTelemetryId);
	}

	#endregion

	#region Methods: Tests

	//[TestMethod]
	//public async Task FromAvailabilityToRequest()
	//{
	//	TelemetryTracker.Operation = new TelemetryOperation(GetOperationId(), $"Availability #{DateTime.UtcNow:yyMMddHHmm}");

	//	// simulate Availability Test
	//	TelemetryTracker.TrackAvailability(DateTime.UtcNow, GetTelemetryId(), "Status", "Passed", TimeSpan.FromMilliseconds(random.Next(100, 150)), true, "West Europe", tags: testServerTags);

	//	// simulate connection delay
	//	await Task.Delay(random.Next(25));

	//	// simulate Request Begin
	//	TelemetryTracker.TrackRequestBegin(GetTelemetryId, out var previousParentId, out var time, out var id);

	//	// simulate execution delay
	//	await Task.Delay(random.Next(25));

	//	// simulat Trace
	//	TelemetryTracker.TrackTrace("Status Requested", SeverityLevel.Information, tags: mainServerTags);

	//	// simulate Request End
	//	TelemetryTracker.TrackRequestEnd(previousParentId, time, id, new Uri("/status", UriKind.Relative), "200", true, TimeSpan.FromMilliseconds(random.Next(50, 100)), "GetStatus", tags: mainServerTags);

	//	// publish data
	//	_ = await TelemetryTracker.PublishAsync();
	//}

	[TestMethod]
	public async Task FromPageViewToRequestTo()
	{
		ClientTelemetryTracker.Operation = new TelemetryOperation(GetOperationId(), $"PageView #{DateTime.UtcNow:yyMMddHHmm}");

		ClientTelemetryTracker.OperationBegin(GetTelemetryId, out var previousParentId, out var pageViewStartTime, out var pageViewId);

		var cancellationToken = TestContext.CancellationTokenSource.Token;

		// make dependency call
		_ = await MakeTelemetryTrackedHttpGetCallAsyc(clientTelemetryTrackedHttpClientHandler, new Uri("https://google.com"), cancellationToken);

		// simulate internal work
		await Task.Delay(Random.Shared.Next(50, 100));

		await SimulateDependencyCallAsync(() => SimulateServerRequestAccept(ClientTelemetryTracker.Operation), HttpMethod.Get, new Uri("https://gostas.dev/int.js"));

		// simulate internal work
		await Task.Delay(Random.Shared.Next(50, 100));

		ClientTelemetryTracker.OperationEnd(previousParentId, pageViewStartTime, out var pageViewDuration);

		// track page view
		var pageView = new PageViewTelemetry(ClientTelemetryTracker.Operation, pageViewStartTime, pageViewId, "Main")
		{
			Duration = pageViewDuration,
			Url = new Uri("https://gostas.dev")
		};

		ClientTelemetryTracker.Add(pageView);

		// publish data
		_ = await ClientTelemetryTracker.PublishAsync(cancellationToken);
	}

	private async Task SimulateDependencyCallAsync(Func<Task> dependency, HttpMethod httpMethod, Uri url)
	{
		ClientTelemetryTracker.OperationBegin(GetTelemetryId, out var previousParentId, out var time, out var id);

		await dependency();

		ClientTelemetryTracker.OperationEnd(previousParentId, time, out var duration);

		// track server request
		ClientTelemetryTracker.TrackDependency(time, id, httpMethod, url, HttpStatusCode.OK, duration);
	}

	public async Task SimulateServerRequestAccept(TelemetryOperation operation)
	{
		ServerTelemetryTracker.Operation = operation;

		ServerTelemetryTracker.OperationBegin(GetTelemetryId, out var previousParentId, out var serverRequestTime, out var serverRequestId);

		var cancellationToken = TestContext.CancellationTokenSource.Token;

		// make dependency call
		_ = await MakeTelemetryTrackedHttpGetCallAsyc(serverTelemetryTrackedHttpClientHandler, new Uri("https://bing.com"), cancellationToken);

		// simulate execution delay
		await Task.Delay(Random.Shared.Next(100));

		// simulat Trace
		ServerTelemetryTracker.TrackTrace("Request from Page View", SeverityLevel.Information);

		ServerTelemetryTracker.OperationEnd(previousParentId, serverRequestTime, out var serverDuration);

		var request = new RequestTelemetry(ServerTelemetryTracker.Operation, serverRequestTime, serverRequestId, new Uri("https://gostas.dev/int.js"), "OK")
		{
			Duration = serverDuration,
			Success = true
		};

		// simulate Request End
		ServerTelemetryTracker.Add(request);

		// publish data
		_ = await ServerTelemetryTracker.PublishAsync(cancellationToken);
	}

	private static async Task<String> MakeTelemetryTrackedHttpGetCallAsyc
	(
		HttpMessageHandler messageHandler,
		Uri uri,
		CancellationToken cancellationToken
	)
	{
		using var httpClient = new HttpClient(messageHandler);

		using var httpResponse = await httpClient.GetAsync(uri, cancellationToken);

		var result = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

		return result;
	}

	private static async Task SimulateTelemetryTrackedHttpCallAsyc
	(
		TelemetryTracker telemetryTracker,
		TimeSpan duration,
		HttpMethod httpMethod,
		Uri uri,
		CancellationToken cancellationToken
	)
	{
		var time = DateTime.UtcNow;

		await Task.Delay(duration, cancellationToken);

		var id = GetTelemetryId();

		telemetryTracker.TrackDependency(time, id, httpMethod, uri, System.Net.HttpStatusCode.OK, DateTime.UtcNow - time);
	}

	#endregion
}
