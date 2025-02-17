// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.IntegrationTests;

using System.Net;
using System.Net.Http;
using System.Text.Json;

using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;

using Microsoft.VisualStudio.TestTools.UnitTesting;

public abstract class AzureIntegrationTestsBase : IDisposable
{
	#region Fields

	private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

	private readonly HttpClient telemetryPublisherHttpClient;

	protected readonly ChainedTokenCredential tokenCredential;

	#endregion

	#region Properties

	public TestContext TestContext { get; }

	protected TelemetryTracker TelemetryTracker { get; private set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Initialize instance.
	/// </summary>
	/// <param name="testContext">Test context.</param>
	public AzureIntegrationTestsBase(TestContext testContext)
	{
		TestContext = testContext;

		//// create token credential
		tokenCredential = new ChainedTokenCredential
		(
			new VisualStudioCredential(),
			new VisualStudioCodeCredential(),
			new ManagedIdentityCredential()
		);

		// create HTTP Client for telemetry publisher
		telemetryPublisherHttpClient = new HttpClient();

		var ingestionEndpoint = new Uri(TestContext.Properties[@"Azure.Monitor.Default.IngestionEndpoint"].ToString());
		var instrumentationKey = new Guid(TestContext.Properties[@"Azure.Monitor.Default.InstrumentationKey"].ToString());

		// create telemetry publisher
		var httpTelemetryPublisher = new HttpTelemetryPublisher
		(
			telemetryPublisherHttpClient,
			ingestionEndpoint,
			instrumentationKey,
			async (cancellationToken) =>
			{
				var tokenRequestContext = new TokenRequestContext(HttpTelemetryPublisher.AuthorizationScopes);

				var token = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken);

				var result = new BearerToken(token.Token, token.ExpiresOn);

				return result;
			}
		);

		// create telemetry tracker
		TelemetryTracker = new TelemetryTracker(telemetryPublishers: httpTelemetryPublisher)
		{
			// create root operation
			Operation = new OperationContext
			{
				Id = Guid.NewGuid().ToString("N"),
				Name = $"Test # {DateTime.UtcNow:dd-hh-mm}"
			}
		};
	}

	#endregion

	#region Methods

	protected static void AssertStandardSuccess(TelemetryPublishResult[] results)
	{
		// check results count
		Assert.AreEqual(1, results.Length, "Results count not 1");

		Assert.IsTrue(results[0] is HttpTelemetryPublishResult);

		// get first
		var result = (HttpTelemetryPublishResult)results[0];

		// check success
		Assert.IsTrue(result.Success, result.Response);

		// check status code
		Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

		// deserialize response
		var response = JsonSerializer.Deserialize<HttpTelemetryPublishResponse>(result.Response, jsonSerializerOptions);

		// check not null
		if (response == null)
		{
			Assert.Fail("Track response can not be deserialized.");

			return;
		}

		Assert.AreEqual(result.Count, response.ItemsAccepted);

		Assert.AreEqual(result.Count, response.ItemsReceived);

		Assert.AreEqual(0, response.Errors.Length);
	}

	public virtual void Dispose()
	{
		telemetryPublisherHttpClient.Dispose();

		GC.SuppressFinalize(this);
	}

	#endregion
}