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

public abstract class IntegrationTestsBase : IDisposable
{
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
		params IReadOnlyCollection<Tuple<String, Boolean, KeyValuePair<String, String>[]>> configList
	)
	{
		TestContext = testContext;

		// create token credential
		TokenCredential = new DefaultAzureCredential();

		var tokenRequestContext = new TokenRequestContext([HttpTelemetryPublisher.AuthorizationScope]);

		var token = TokenCredential.GetTokenAsync(tokenRequestContext, CancellationToken.None).Result;

		telemetryPublishHttpClient = new HttpClient();

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
				publisher = new HttpTelemetryPublisher(telemetryPublishHttpClient, ingestionEndpoint, instrumentationKey, tags: publisherTags);
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

				publisher = new HttpTelemetryPublisher(telemetryPublishHttpClient, ingestionEndpoint, instrumentationKey, getAccessToken, publisherTags);
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
		telemetryPublishHttpClient.Dispose();

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

			Assert.AreEqual(0, response.Errors.Count, nameof(HttpTelemetryPublishResponse.Errors));
		}
	}

	protected static async Task<String> MakeDependencyCallAsyc
	(
		HttpMessageHandler messageHandler,
		Uri uri,
		CancellationToken cancellationToken
	)
	{
		using var httpClient = new HttpClient(messageHandler, false);

		using var httpResponse = await httpClient.GetAsync(uri, cancellationToken);

		var result = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

		return result;
	}

	#endregion

	#region Methods: Simulate Telemetry

	protected static async Task SimulateAvailabilityAsync
	(
		TelemetryTracker telemetryTrakcer,
		String name,
		String message,
		Boolean success,
		String? runLocation,
		Func<CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin operation
		var operationInfo = telemetryTrakcer.TrackOperationBegin(GetTelemetryId);

		// execute subsequent
		await subsequent(cancellationToken);

		// end operation
		telemetryTrakcer.TrackAvailabilityEnd(operationInfo, name, message, success, runLocation);
	}

	protected static async Task SimulateDependencyAsync
	(
		TelemetryTracker telemetryTrakcer,
		HttpMethod httpMethod,
		Uri url,
		HttpStatusCode statusCode,
		Func<CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin operation
		var operationInfo = telemetryTrakcer.TrackOperationBegin(GetTelemetryId);

		// execute subsequent
		await subsequent(cancellationToken);

		// end operation
		telemetryTrakcer.TrackDependencyEnd(operationInfo, httpMethod, url, statusCode, (Int32) statusCode < 399);
	}

	protected static async Task SimulatePageViewAsync
	(
		TelemetryTracker telemetryTracker,
		String pageName,
		Uri pageUrl,
		Func<CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin operation
		var operationInfo = telemetryTracker.TrackOperationBegin(GetTelemetryId);

		// execute subsequent
		await subsequent(cancellationToken);

		// end operation
		telemetryTracker.TrackPageViewEnd(operationInfo, pageName, pageUrl);
	}

	public static async Task SimulateRequestAsync
	(
		TelemetryTracker telemetryTracker,
		Uri url,
		String responseCode,
		Boolean success,
		Func<CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin operation
		var operationInfo = telemetryTracker.TrackOperationBegin(GetTelemetryId);

		// execute subsequent
		await subsequent(cancellationToken);

		// end operation
		telemetryTracker.TrackRequestEnd(operationInfo, url, responseCode, success);
	}

	#endregion
}