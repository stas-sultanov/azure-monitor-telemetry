// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.IntegrationTests;
using Azure.Monitor.Telemetry.Dependency;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// The goals of this test:
/// - publish telemetry data into two instances of AppInsights; one with auth, one without auth.
/// - test dependency tracking with <see cref="TelemetryTrackedHttpClientHandler"/>.
/// </summary>
[TestCategory("IntegrationTests")]
[TestClass]
public sealed class EndToEndTests : AzureIntegrationTestsBase
{
	private const String QueueName = "commands";

	#region Data

	private static readonly Random random = new(DateTime.UtcNow.Millisecond);

	private readonly TelemetryTrackedHttpClientHandler telemetryTrackedHttpClientHandler;

	private readonly KeyValuePair<String, String>[] cilentTags = [new(TelemetryTagKey.DeviceType, "Browser"), new(TelemetryTagKey.LocationIp, "78.26.233.104")];

	private readonly KeyValuePair<String, String>[] mainServerTags = [new(TelemetryTagKey.CloudRole, "Main Service"), new(TelemetryTagKey.CloudRoleInstance, "1"), new(TelemetryTagKey.LocationIp, "4.210.128.4")];

	private readonly KeyValuePair<String, String>[] testServerTags = [new(TelemetryTagKey.CloudRoleInstance, "Alpha"), new(TelemetryTagKey.LocationIp, "20.33.2.8")];

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="DependencyTrackingTests"/> class.
	/// </summary>
	/// <param name="testContext">The test context.</param>
	public EndToEndTests(TestContext testContext)
		: base
		(
			testContext,
			[],
			[
				Tuple.Create(@"Azure.Monitor.AuthOn.", true, Array.Empty<KeyValuePair<String, String>>()),
			]
		)
	{
		telemetryTrackedHttpClientHandler = new TelemetryTrackedHttpClientHandler(TelemetryTracker, GetTelemetryId);
	}

	#endregion

	#region Methods: Tests

	[TestMethod]
	public async Task FromAvailabilityToRequest()
	{
		TelemetryTracker.Operation = new TelemetryOperation(GetOperationId(), $"Availability #{DateTime.UtcNow:yyMMddHHmm}");

		// simulate Availability Test
		TelemetryTracker.TrackAvailability(DateTime.UtcNow, GetTelemetryId(), "Status", "Passed", TimeSpan.FromMilliseconds(random.Next(100, 150)), true, "West Europe", tags: testServerTags);

		// simulate connection delay
		await Task.Delay(random.Next(25));

		// simulate Request Begin
		TelemetryTracker.TrackRequestBegin(GetTelemetryId, out var previousParentId, out var time, out var id);

		// simulate execution delay
		await Task.Delay(random.Next(25));

		// simulat Trace
		TelemetryTracker.TrackTrace("Status Requested", SeverityLevel.Information, tags: mainServerTags);

		// simulate Request End
		TelemetryTracker.TrackRequestEnd(previousParentId, time, id, new Uri("/status", UriKind.Relative), "200", true, TimeSpan.FromMilliseconds(random.Next(50, 100)), "GetStatus", tags: mainServerTags);

		// publish data
		_ = await TelemetryTracker.PublishAsync();
	}

	[TestMethod]
	public async Task FromPageViewToRequestTo()
	{
		var cancellationToken = TestContext.CancellationTokenSource.Token;

		var mainPageRelativeUri = new Uri("https://gostas.dev");

		TelemetryTracker.Operation = new TelemetryOperation(GetOperationId(), $"PageView #{DateTime.UtcNow:yyMMddHHmm}");

		// simulate page view
		var pageView = new PageViewTelemetry(TelemetryTracker.Operation, DateTime.UtcNow, GetTelemetryId(), "Main")
		{
			Duration = TimeSpan.FromMilliseconds(random.Next(150, 250)),
			Url = mainPageRelativeUri,
			Tags = cilentTags
		};

		// simulate request delay
		await Task.Delay(random.Next(50), cancellationToken);

		// simulate Request Begin
		TelemetryTracker.TrackRequestBegin(GetTelemetryId, out var previousParentId, out var time, out var id);

		// simulate execution delay
		await Task.Delay(random.Next(50), cancellationToken);

		// simulat Trace
		TelemetryTracker.TrackTrace("Page View Requested", SeverityLevel.Information, tags: mainServerTags);

		_ = await MakeTelemetryTrackedHttpGetCallAsyc("https://google.com", cancellationToken);

		// simulate Request End
		TelemetryTracker.TrackRequestEnd(previousParentId, time, id, mainPageRelativeUri, "200", true, TimeSpan.FromMilliseconds(random.Next(50, 100)), "GET /", tags: mainServerTags);

		TelemetryTracker.Add(pageView);

		// publish data
		_ = await TelemetryTracker.PublishAsync(cancellationToken);
	}

	private async Task<String> MakeTelemetryTrackedHttpGetCallAsyc(String uri, CancellationToken cancellationToken)
	{
		using var httpClient = new HttpClient(telemetryTrackedHttpClientHandler);

		using var httpResponse = await httpClient.GetAsync(uri, cancellationToken);

		var result = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

		return result;
	}

	#endregion
}
