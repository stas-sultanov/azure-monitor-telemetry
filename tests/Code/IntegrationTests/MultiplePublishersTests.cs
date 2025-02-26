// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.IntegrationTests;

using System.Diagnostics;

using Azure.Core.Pipeline;
using Azure.Monitor.Telemetry.Dependency;
using Azure.Storage.Queues;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// The goals of this test:
/// - publish telemetry data into two instances of AppInsights; one with auth, one without auth.
/// - test dependency tracking with <see cref="TelemetryTrackedHttpClientHandler"/>.
/// </summary>
[TestCategory("IntegrationTests")]
[TestClass]
public sealed class MultiplePublishersTests : AzureIntegrationTestsBase
{
	private const String QueueName = "commands";

	#region Data

	private static readonly Random random = new(DateTime.UtcNow.Millisecond);

	private readonly HttpClientTransport queueClientHttpClientTransport;

	private readonly QueueClient queueClient;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="DependencyTrackingTests"/> class.
	/// </summary>
	/// <param name="testContext">The test context.</param>
	public MultiplePublishersTests(TestContext testContext)
		: base
		(
			testContext,
			[],
			[
				Tuple.Create(@"Azure.Monitor.AuthOn.", true, Array.Empty<KeyValuePair<String, String>>()),
				Tuple.Create(@"Azure.Monitor.AuthOff.", false, Array.Empty<KeyValuePair<String, String>>())
			]
		)
	{
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
		TelemetryTracker.TrackRequestBegin(GetOperationId, out var previousParentId, out var time, out var id);

		var cancellationToken = TestContext.CancellationTokenSource.Token;

		// execute
		await SendMessageTrackedAsync("begin", cancellationToken);

		await Task.Delay(random.Next(300));

		await SendMessageTrackedAsync("end", cancellationToken);

		TelemetryTracker.TrackRequestEnd
		(
			previousParentId,
			time,
			id,
			new Uri($"tst:{nameof(DependencyTrackingTests)}"),
			"OK",
			true,
			DateTime.UtcNow - time,
			nameof(AzureQueue_Success)
		);

		var result = await TelemetryTracker.PublishAsync(cancellationToken);

		// assert
		AssertStandardSuccess(result);
	}

	private async Task SendMessageTrackedAsync(String message, CancellationToken cancellationToken)
	{
		TelemetryTracker.TrackDependencyInProcBegin(GetOperationId, out var previousParentId, out var time, out var id);

		_ = await queueClient.SendMessageAsync(message, cancellationToken);

		TelemetryTracker.TrackDependencyInProcEnd(previousParentId, time, id, "Storage", true, DateTime.UtcNow - time);
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
