// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.TelemetryTests;

using System.Diagnostics;

using Azure.Core.Pipeline;
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Dependency;
using Azure.Storage.Queues;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// The goals of this test suite:
/// - test dependency tracking with <see cref="TelemetryTrackedHttpClientHandler"/>.
/// </summary>
[TestCategory("IntegrationTests")]
[TestClass]
public sealed class DependencyTrackingTests : IntegrationTestsBase
{
	private const String QueueName = "commands";

	#region Fields

	private readonly QueueClient queueClient;
	private readonly HttpClientTransport queueClientHttpClientTransport;
	private readonly TelemetryClient telemetryClient;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="DependencyTrackingTests"/> class.
	/// </summary>
	/// <param name="testContext">The test context.</param>
	public DependencyTrackingTests
	(
		in TestContext testContext
	)
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
		telemetryClient = new TelemetryClient(TelemetryPublishers)
		{
			Context = new TelemetryTags
			{
				CloudRole = "Tester",
				CloudRoleInstance = Environment.MachineName,
			}
		};

		var handler = new TelemetryTrackedHttpClientHandler(telemetryClient, () => ActivitySpanId.CreateRandom().ToString());

		queueClientHttpClientTransport = new HttpClientTransport(handler);

		var queueServiceUriParamName = "Azure.Queue.Default.ServiceUri";
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

	#region Methods: Implementation of IDisposable

	/// <inheritdoc/>
	public override void Dispose()
	{
		queueClientHttpClientTransport.Dispose();

		base.Dispose();
	}

	#endregion

	#region Methods: Tests

	[TestMethod]
	public async Task AzureQueue()
	{
		// set operation
		telemetryClient.Context = telemetryClient.Context with
		{
			OperationId = TelemetryFactory.GetOperationId(),
			OperationName = $"{nameof(DependencyTrackingTests)}.{nameof(AzureQueue)}"
		};

		telemetryClient.ActivityScopeBegin(TelemetryFactory.GetOperationId, out var time, out var timestamp, out var id, out var context);

		var cancellationToken = TestContext.CancellationTokenSource.Token;

		// execute
		await SendMessageTrackedAsync("begin", cancellationToken);

		await Task.Delay(500);

		await SendMessageTrackedAsync("middle", cancellationToken);

		await Task.Delay(500);

		await SendMessageTrackedAsync("end", cancellationToken);

		telemetryClient.ActivityScopeEnd(context, timestamp, out var duration);

		telemetryClient.TrackRequest
		(
			time,
			duration,
			id,
			new Uri($"tst:{nameof(DependencyTrackingTests)}"),
			"OK",
			true,
			nameof(AzureQueue)
		);

		var result = await telemetryClient.PublishAsync(cancellationToken);

		// assert
		AssertStandardSuccess(result);
	}

	#endregion

	#region Methods: Helpers

	private async Task SendMessageTrackedAsync(String message, CancellationToken cancellationToken)
	{
		telemetryClient.ActivityScopeBegin(TelemetryFactory.GetActivityId, out var time, out var timestamp, out var id, out var context);

		_ = await queueClient.SendMessageAsync(message, cancellationToken);

		telemetryClient.ActivityScopeEnd(context, timestamp, out var duration);

		telemetryClient.TrackDependencyInProc(time, duration, id, "Storage", true);
	}

	#endregion
}
