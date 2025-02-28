// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Publish;

using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

/// <summary>
/// Provides telemetry publishing using HTTP protocol.
/// </summary>
/// <remarks>
/// Handles publishing of telemetry data to Azure Monitor's ingestion endpoints.
/// It supports both authenticated (with Bearer token) and unauthenticated scenarios.
/// For authenticated scenarios, uses v2.1 of the track endpoint.
/// For unauthenticated scenarios, uses the v2 of the track endpoint.
/// For authenticated scenario, the identity, on behalf of which the operation occurs, requires <a href="https://learn.microsoft.com/azure/role-based-access-control/built-in-roles/monitor#monitoring-metrics-publisher">Monitoring Metrics Publisher</a> role.
/// </remarks>
public sealed class HttpTelemetryPublisher : TelemetryPublisher
{
	#region Constants

	/// <summary>
	/// Name of the HTTP Authorization header.
	/// </summary>
	private const String AuthorizationHeaderName = @"Authorization";

	/// <summary>
	/// Prefix for the HTTP Authorization header value.
	/// </summary>
	private const String AuthorizationHeaderValuePrefix = @"Bearer ";

	/// <summary>
	/// Value of the Content-Encoding header.
	/// </summary>
	private const String ContentEncodingHeaderValue = "gzip";

	/// <summary>
	/// The authorization scope for the Azure Monitor.
	/// </summary>
	public const String AuthorizationScope = "https://monitor.azure.com//.default";

	private static readonly UTF8Encoding encoding = new(false);

	private static readonly MediaTypeHeaderValue contentTypeHeaderValue = MediaTypeHeaderValue.Parse(@"application/x-json-stream");

	#endregion

	#region Fields

	private DateTimeOffset authorizationTokenExpiresOn;
	private String? authorizationHeaderValue;
	private readonly Func<CancellationToken, Task<BearerToken>>? getAccessToken;
	private readonly HttpClient httpClient;
	private readonly Uri ingestionEndpoint;
	private readonly String instrumentationKey;
	private readonly IReadOnlyList<KeyValuePair<String, String>>? tags;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="HttpTelemetryPublisher"/> class.
	/// </summary>
	/// <param name="httpClient">The HTTP client to publish telemetry data.</param>
	/// <param name="ingestionEndpoint">The URI endpoint where telemetry data will be sent. Must be an absolute, non-file, non-UNC URI.</param>
	/// <param name="instrumentationKey">The instrumentation key used to authenticate with the telemetry service. Cannot be an empty GUID.</param>
	/// <param name="getAccessToken">Function to get a bearer token for authentication. If not provided, no authentication will be used. Is optional.</param>
	/// <param name="tags">A read-only list of tags to attach to each telemetry item. Is optional.</param>
	/// <exception cref="ArgumentException">If <paramref name="ingestionEndpoint"/> is not valid absolute uri.</exception>
	/// <exception cref="ArgumentException">If <paramref name="instrumentationKey"/> is empty.</exception>
	public HttpTelemetryPublisher
	(
		HttpClient httpClient,
		Uri ingestionEndpoint,
		Guid instrumentationKey,
		Func<CancellationToken, Task<BearerToken>>? getAccessToken = null,
		IReadOnlyList<KeyValuePair<String, String>>? tags = null
	)
	{
		if (!ingestionEndpoint.IsAbsoluteUri || ingestionEndpoint.IsFile || ingestionEndpoint.IsUnc)
		{
			throw new ArgumentException("Not valid.", nameof(ingestionEndpoint));
		}

		if (instrumentationKey == Guid.Empty)
		{
			throw new ArgumentException("Not valid.", nameof(instrumentationKey));
		}

		this.httpClient = httpClient;

		this.ingestionEndpoint = new Uri(ingestionEndpoint, getAccessToken == null ? @"v2/track" : @"v2.1/track");

		this.instrumentationKey = instrumentationKey.ToString();

		this.getAccessToken = getAccessToken;

		this.tags = tags;
	}

	#endregion

	#region Methods

	/// <inheritdoc/>
	public async Task<TelemetryPublishResult> PublishAsync
	(
		IReadOnlyList<Telemetry> telemetryList,
		IReadOnlyList<KeyValuePair<String, String>>? trackerTags,
		CancellationToken cancellationToken
	)
	{
		// create memory stream to write request
		using var memoryStream = new MemoryStream();

		// we must keep open memory stream to read from it after write
		using (var compressedStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
		{
			// create stream writer based on memory stream as we want to write text in JSON format
			using var streamWriter = new StreamWriter(compressedStream, encoding);

			for (var index = 0; index < telemetryList.Count; index++)
			{
				var telemetryItem = telemetryList[index];

				JsonTelemetrySerializer.Serialize(streamWriter, instrumentationKey, telemetryItem, trackerTags, tags);
			}
		}

		// reset memory stream position to read it from the beginning
		memoryStream.Position = 0;

		// create http request message
		using var request = new HttpRequestMessage(HttpMethod.Post, ingestionEndpoint);

		// create request content based on memory stream
		request.Content = new StreamContent(memoryStream);

		// check if authorization is configured
		if (getAccessToken != null)
		{
			// check if token has been ever requested or already has expired
			if (authorizationHeaderValue == null || DateTimeOffset.UtcNow > authorizationTokenExpiresOn)
			{
				// get token
				var token = await getAccessToken(cancellationToken).ConfigureAwait(false);

				authorizationTokenExpiresOn = token.ExpiresOn;

				authorizationHeaderValue = AuthorizationHeaderValuePrefix + token.Value;
			}

			// add authorization header to the request
			_ = request.Headers.TryAddWithoutValidation(AuthorizationHeaderName, authorizationHeaderValue);
		}

		// set Content-Type. actually works without it, but we should be consistent
		request.Content.Headers.ContentType = contentTypeHeaderValue;

		// set Content-Encoding.
		request.Content.Headers.ContentEncoding.Add(ContentEncodingHeaderValue);

		// record time
		var httpRequestTime = DateTime.UtcNow;

		// start timer to measure how long it takes to execute the request
		var httpRequestTimer = System.Diagnostics.Stopwatch.StartNew();

		// execute http request to the ingestion endpoint
		using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

		// stop timer
		httpRequestTimer.Stop();

		// read response content as string
#if NET462
		var responseContentAsString = await response.Content.ReadAsStringAsync();
#else
		var responseContentAsString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
		// create result
		var result = new HttpTelemetryPublishResult
		{
			Count = telemetryList.Count,
			Duration = httpRequestTimer.Elapsed,
			Response = responseContentAsString,
			StatusCode = response.StatusCode,
			Success = response.IsSuccessStatusCode,
			Time = httpRequestTime,
			Url = ingestionEndpoint
		};

		return result;
	}

	#endregion
}
