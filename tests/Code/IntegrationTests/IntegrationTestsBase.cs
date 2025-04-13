// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Tests;

using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;

public abstract class IntegrationTestsBase : IDisposable
{
	#region Types

	public sealed class PublisherConfiguration
	{
		public required Boolean Authenticate { get; init; }

		public required String ConfigPrefix { get; init; }
	}

	#endregion

	#region Fields

	private static readonly JsonSerializerOptions jsonSerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly HttpClient telemetryPublishHttpClient;

	#endregion

	#region Properties

	/// <summary>
	/// Test context.
	/// </summary>
	protected TestContext TestContext { get; }

	/// <summary>
	/// Collection of telemetry publishers initialized from the configuration.
	/// </summary>
	protected IReadOnlyList<TelemetryPublisher> TelemetryPublishers { get; }

	/// <summary>
	/// The token credential used to authenticate calls to Azure resources.
	/// </summary>
	protected DefaultAzureCredential TokenCredential { get; }

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="IntegrationTestsBase"/> class.
	/// </summary>
	/// <param name="testContext">The test context</param>
	/// <param name="configList">List of configurations.</param>
	public IntegrationTestsBase
	(
		TestContext testContext,
		params IReadOnlyList<PublisherConfiguration> configList
	)
	{
		TestContext = testContext;

		// create token credential
		TokenCredential = new DefaultAzureCredential();

		var tokenRequestContext = new TokenRequestContext([HttpTelemetryPublisher.AuthorizationScope]);

		var token = TokenCredential.GetToken(tokenRequestContext);

		telemetryPublishHttpClient = new HttpClient();

		TelemetryPublishers = [.. configList.Select(config => InitializePublisherFromConfig(token, config))];
	}

	private TelemetryPublisher InitializePublisherFromConfig(AccessToken token, PublisherConfiguration config)
	{
		var ingestionEndpointParamName = config.ConfigPrefix + "IngestionEndpoint";
		var ingestionEndpointParam = TestContext.Properties[ingestionEndpointParamName]?.ToString() ?? throw new ArgumentException($"Parameter {ingestionEndpointParamName} has not been provided.");
		var ingestionEndpoint = new Uri(ingestionEndpointParam);

		var instrumentationKeyParamName = config.ConfigPrefix + "InstrumentationKey";
		var instrumentationKeyParam = TestContext.Properties[instrumentationKeyParamName]?.ToString() ?? throw new ArgumentException($"Parameter {instrumentationKeyParamName} has not been provided.");
		var instrumentationKey = new Guid(instrumentationKeyParam);

		TelemetryPublisher publisher;

		if (config.Authenticate)
		{
			Task<BearerToken> getAccessToken(CancellationToken cancellationToken)
			{
				var result = new BearerToken
				{
					ExpiresOn = token.ExpiresOn,
					Value = token.Token
				};

				return Task.FromResult(result);
			}

			publisher = new HttpTelemetryPublisher(telemetryPublishHttpClient, ingestionEndpoint, instrumentationKey, getAccessToken);
		}
		else
		{
			publisher = new HttpTelemetryPublisher(telemetryPublishHttpClient, ingestionEndpoint, instrumentationKey);
		}

		return publisher;
	}

	#endregion

	#region Methods: Implementation of IDisposable

	/// <inheritdoc/>
	public virtual void Dispose()
	{
		telemetryPublishHttpClient.Dispose();

		GC.SuppressFinalize(this);
	}

	#endregion

	#region Methods

	protected static void AssertStandardSuccess(TelemetryPublishResult[] telemetryPublishResults)
	{
		foreach (var telemetryPublishResult in telemetryPublishResults)
		{
			var result = telemetryPublishResult as HttpTelemetryPublishResult;

			Assert.IsNotNull(result, $"Result is not of {nameof(HttpTelemetryPublishResult)} type.");

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

			Assert.AreEqual(result.Count, response.ItemsAccepted, nameof(HttpTelemetryPublishResponse.ItemsAccepted));

			Assert.AreEqual(result.Count, response.ItemsReceived, nameof(HttpTelemetryPublishResponse.ItemsReceived));

			Assert.AreEqual(0, response.Errors.Count, nameof(HttpTelemetryPublishResponse.Errors));
		}
	}

	#endregion
}
