// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.IntegrationTests;

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Telemetry;
using Azure.Monitor.Telemetry.Publish;

using Microsoft.VisualStudio.TestTools.UnitTesting;

public abstract class AzureIntegrationTestsBase : IDisposable
{
	#region Fields

	private static readonly JsonSerializerOptions jsonSerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly HttpClient httpClient;

	#endregion

	#region Properties

	protected TestContext TestContext { get; }

	protected IReadOnlyList<TelemetryPublisher> TelemetryPublishers { get; }

	protected DefaultAzureCredential TokenCredential { get; }

	#endregion

	#region Constructors

	/// <summary>
	/// Initialize instance.
	/// </summary>
	/// <param name="testContext">Test context.</param>
	/// <param name="configKeyPrefixList">The list of configuration key prefixes</param>
	public AzureIntegrationTestsBase
	(
		TestContext testContext,
		params Tuple<String, Boolean, KeyValuePair<String, String>[]>[] configList
	)
	{
		TestContext = testContext;

		// create token credential
		TokenCredential = new DefaultAzureCredential();

		var tokenRequestContext = new TokenRequestContext(HttpTelemetryPublisher.AuthorizationScopes);

		var token = TokenCredential.GetTokenAsync(tokenRequestContext, CancellationToken.None).Result;

		httpClient = new HttpClient();

		var telemetryPublishers = new List<TelemetryPublisher>();

		foreach (var config in configList)
		{
			var ingestionEndpointParamName = config.Item1 + "IngestionEndpoint";
			var ingestionEndpointParam = TestContext.Properties[ingestionEndpointParamName]?.ToString() ?? throw new ArgumentException($"Parameter {ingestionEndpointParamName} has not been provided.");
			var ingestionEndpoint = new Uri(ingestionEndpointParam);

			var instrumentationKeyParamName = config.Item1 + "InstrumentationKey";
			var instrumentationKeyParam = TestContext.Properties[instrumentationKeyParamName]?.ToString() ?? throw new ArgumentException($"Parameter {instrumentationKeyParamName} has not been provided.");
			var instrumentationKey = new Guid(instrumentationKeyParam);

			var publisherTags = config.Item3;

			TelemetryPublisher publisher;

			if (!config.Item2)
			{
				publisher = new HttpTelemetryPublisher(httpClient, ingestionEndpoint, instrumentationKey, tags: publisherTags);
			}
			else
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

				publisher = new HttpTelemetryPublisher(httpClient, ingestionEndpoint, instrumentationKey, getAccessToken, publisherTags);
			}

			telemetryPublishers.Add(publisher);
		}

		TelemetryPublishers = telemetryPublishers.AsReadOnly();
	}

	#endregion

	#region Methods: Implementation of IDisposable

	/// <inheritdoc/>
	public virtual void Dispose()
	{
		httpClient.Dispose();

		GC.SuppressFinalize(this);
	}

	#endregion

	#region Methods

	protected static String GetOperationId()
	{
		return ActivityTraceId.CreateRandom().ToString();
	}

	protected static String GetTelemetryId()
	{
		return ActivitySpanId.CreateRandom().ToString();
	}

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

			Assert.AreEqual(0, response.Errors.Length, nameof(HttpTelemetryPublishResponse.Errors));
		}
	}

	#endregion
}