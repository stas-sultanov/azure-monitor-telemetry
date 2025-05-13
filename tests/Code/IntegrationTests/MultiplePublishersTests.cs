// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.TelemetryTests;

using Azure.Monitor.Telemetry;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// The goals of this test:
/// - publish telemetry data into two instances of AppInsights; one with auth, one without auth.
/// </summary>
[TestCategory("IntegrationTests")]
[TestClass]
public sealed class MultiplePublishersTests : IntegrationTestsBase
{
	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="DependencyTrackingTests"/> class.
	/// </summary>
	/// <param name="testContext">The test context.</param>
	public MultiplePublishersTests
	(
		TestContext testContext
	)
		: base
		(
			testContext,
			new PublisherConfiguration()
			{
				ConfigPrefix = "Azure.Monitor.AuthOn.",
				Authenticate = true
			},
			new PublisherConfiguration()
			{
				ConfigPrefix = "Azure.Monitor.AuthOff.",
				Authenticate = false
			}
		)
	{
		TelemetryClient = new TelemetryClient(TelemetryPublishers)
		{
			Context = new()
			{
				CloudRole = "Tester",
				CloudRoleInstance = Environment.MachineName
			}
		};
	}

	#endregion

	#region Properties

	private TelemetryClient TelemetryClient { get; }

	#endregion

	#region Methods: Tests

	[TestMethod]
	public async Task PublishSomeTelemetryAsync()
	{
		// set context
		TelemetryClient.Context = TelemetryClient.Context with
		{
			OperationId = TelemetryFactory.GetOperationId(),
			OperationName = nameof(MultiplePublishersTests),
		};

		TelemetryClient.TrackEvent("start");

		TelemetryClient.TrackTrace("started", SeverityLevel.Verbose);

		_ = await TelemetryClient.PublishAsync();
	}

	#endregion
}
