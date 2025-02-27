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
/// This handler uses a <see cref="TelemetryTracker"/> to track details about HTTP requests and responses, including the request URI, method, status code, and duration.
/// </remarks>
/// <param name="telemetryTracker">The telemetry tracker.</param>
/// <param name="getId">A function that returns a unique identifier for the telemetry operation.</param>
public class TelemetryTrackedHttpClientHandler
(
	TelemetryTracker telemetryTracker,
	Func<String> getId
)
	: HttpClientHandler
{
	/// <summary>
	/// A delegate that returns an identifier for the <see cref="DependencyTelemetry"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException"> when <paramref name="getId"/> is null.</exception>
	private readonly Func<String> getId = getId;

	/// <summary>
	/// The telemetry tracker to track outgoing HTTP requests.
	/// </summary>
	/// <exception cref="ArgumentNullException"> when <paramref name="telemetryTracker"/> is null.</exception>
	private readonly TelemetryTracker telemetryTracker = telemetryTracker;

	/// <inheritdoc/>
	protected override async Task<HttpResponseMessage> SendAsync
	(
		HttpRequestMessage request,
		CancellationToken cancellationToken
	)
	{
		// capture current time
		var time = DateTime.UtcNow;

		// start a timer to measure the duration of the request
		var timer = Stopwatch.StartNew();

		HttpResponseMessage result;

		try
		{
			// send the HTTP request and capture the response.
			result = await base.SendAsync(request, cancellationToken);
		}
		finally
		{
			timer.Stop();
		}

		// get id for the current dependency telemetry operation
		var id = getId();

		// track telemetry
		telemetryTracker.TrackDependency(time, id, request.Method, request.RequestUri ?? new Uri("http:none"), result.StatusCode, timer.Elapsed);

		return result;
	}
}
