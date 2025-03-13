// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Dependency;

using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A custom <see cref="HttpClientHandler"/> that enables tracking telemetry of HTTP requests.
/// </summary>
/// <remarks>
/// This handler uses a <see cref="TelemetryClient"/> to track details about HTTP requests and responses, including the request URI, method, status code, and duration.
/// </remarks>
/// <param name="telemetryClient">The telemetry tracker.</param>
/// <param name="getActivityId">A function that returns a unique identifier for the activity.</param>
public class TelemetryTrackedHttpClientHandler
(
	TelemetryClient telemetryClient,
	Func<String> getActivityId
)
	: HttpClientHandler
{
	/// <summary>
	/// A function that returns a unique identifier for the activity.
	/// </summary>
	private readonly Func<String> getActivityId = getActivityId;

	/// <summary>
	/// The telemetry tracker.
	/// </summary>
	private readonly TelemetryClient telemetryClient = telemetryClient;

	/// <inheritdoc/>
	protected override async Task<HttpResponseMessage> SendAsync
	(
		HttpRequestMessage request,
		CancellationToken cancellationToken
	)
	{
		// start stopwatch
		var stopwatch = Stopwatch.StartNew();

		// get time
		var time = DateTime.UtcNow;

		// get activity id
		var id = getActivityId();

		// send the HTTP request and capture the response.
		var result = await base.SendAsync(request, cancellationToken);

		// stop stopwatch
		stopwatch.Stop();

		// get duration
		var duration = stopwatch.Elapsed;

		// track telemetry
		// if RequestUri is null the host class will throw exception before calling this method
		telemetryClient.TrackDependencyHttp
		(
			time,
			duration,
			id,
			request.Method,
			request.RequestUri!,
			result.StatusCode,
			result.IsSuccessStatusCode
		);

		return result;
	}
}
