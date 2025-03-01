// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.IntegrationTests;

using System.Diagnostics;

using Azure.Core.Pipeline;
using Azure.Monitor.Telemetry.Dependency;
using Azure.Monitor.Telemetry.Tests;
using Azure.Storage.Queues;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// The goals of this test:
/// - publish telemetry data into two instances of AppInsights; one with auth, one without auth.
/// - test dependency tracking with <see cref="TelemetryTrackedHttpClientHandler"/>.
/// </summary>
[TestCategory("IntegrationTests")]
[TestClass]
public sealed class DependencyTrackingTests : IntegrationTestsBase
{
	private const String QueueName = "commands";

	#region Data

	private readonly HttpClientTransport queueClientHttpClientTransport;

	private readonly QueueClient queueClient;

	private TelemetryTracker TelemetryTracker { get; }

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="DependencyTrackingTests"/> class.
	/// </summary>
	/// <param name="testContext">The test context.</param>
	public DependencyTrackingTests(TestContext testContext)
		: base
		(
			testContext,
			new PublisherConfiguration()
			{
				ConfigPrefx = @"Azure.Monitor.AuthOn.",
				UseAuthentication = true
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

		var handler = new TelemetryTrackedHttpClientHandler(TelemetryTracker, () => ActivitySpanId.CreateRandom().ToString());

		queueClientHttpClientTransport = new HttpClientTransport(handler);

		var queueServiceUriParamName = @"Azure.Queue.Default.ServiceUri";
		var queueServiceUriParam = TestContext.Properties[queueServiceUriParamName]?.ToString() ?? throw new ArgumentException($"Parameter {queueServiceUriParamName} has not been provided.");
		var queueServiceUri = new Uri(queueServiceUriParam);

		var queueClientOptions = new QueueClientOptions()
		{
			MessageEncoding = QueueMessageEncoding.Base64,
			Transport = queueClientHttpClientTransport
		};

		var queueService = new QueueServiceClient(queueServiceUri, TokenCredential, queueClientOptions);

		queueClient = queueService.GetQueueClient(QueueName);
	}

	#endregion

	#region Methods: Tests

	[TestMethod]
	public async Task AzureQueue_Success()
	{
		var context = TelemetryTracker.TrackOperationBegin(GetOperationId);

		var cancellationToken = TestContext.CancellationTokenSource.Token;

		// execute
		await SendMessageTrackedAsync("begin", cancellationToken);

		await Task.Delay(500);

		await SendMessageTrackedAsync("middle", cancellationToken);

		await Task.Delay(500);

		await SendMessageTrackedAsync("end", cancellationToken);

		TelemetryTracker.TrackRequestEnd
		(
			context,
			new Uri($"tst:{nameof(DependencyTrackingTests)}"),
			"OK",
			true,
			nameof(AzureQueue_Success)
		);

		var result = await TelemetryTracker.PublishAsync(cancellationToken);

		// assert
		AssertStandardSuccess(result);
	}

	private async Task SendMessageTrackedAsync(String message, CancellationToken cancellationToken)
	{
		var operationContext = TelemetryTracker.TrackOperationBegin(GetTelemetryId);

		_ = await queueClient.SendMessageAsync(message, cancellationToken);

		TelemetryTracker.TrackDependencyInProcEnd(operationContext, "Storage", true);
	}

	#endregion

	#region Methods: Implementation of IDisposable

	/// <inheritdoc/>
	public override void Dispose()
	{
		queueClientHttpClientTransport.Dispose();

		base.Dispose();
	}

	#endregion
}
