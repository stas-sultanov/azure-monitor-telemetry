﻿// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.IntegrationTests;

using System.Diagnostics;

using Azure.Monitor.Telemetry.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// The goals of this test:
/// - publish telemetry data into two instances of AppInsights; one with auth, one without auth.
/// </summary>
[TestCategory("IntegrationTests")]
[TestClass]
public sealed class MultiplePublishersTests : IntegrationTestsBase
{
	#region Data

	private TelemetryClient TelemetryClient { get; }

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
			new PublisherConfiguration()
			{
				ConfigPrefix = @"Azure.Monitor.AuthOn.",
				UseAuthentication = true
			},
			new PublisherConfiguration()
			{
				ConfigPrefix = @"Azure.Monitor.AuthOff.",
				UseAuthentication = false
			}
		)
	{
		TelemetryClient = new TelemetryClient
		(
			TelemetryPublishers,
			[
				new (TelemetryTagKeys.CloudRole, "Tester"),
				new (TelemetryTagKeys.CloudRoleInstance, Environment.MachineName)
			]
		);
	}

	#endregion

	#region Methods: Tests

	[TestMethod]
	public async Task PublishSomeTelemetryAsync()
	{
		TelemetryClient.Operation = new()
		{
			Id = ActivityTraceId.CreateRandom().ToString(),
			Name = nameof(MultiplePublishersTests)
		};

		TelemetryClient.TrackEvent("start");

		TelemetryClient.TrackTrace("started", SeverityLevel.Verbose);

		_ = await TelemetryClient.PublishAsync();
	}

	#endregion
}
