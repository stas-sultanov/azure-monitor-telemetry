// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Dependency;

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

	private readonly TelemetryTracker telemetryTracker = telemetryTracker;

	/// <inheritdoc/>
	/// <exception cref="ArgumentNullException"> if <paramref name="request"/> is null.</exception>
	/// <exception cref="ArgumentException"> if <paramref name="request"/> property <see cref="HttpRequestMessage.RequestUri"/> is null.</exception>
	protected override async Task<HttpResponseMessage> SendAsync
	(
		HttpRequestMessage request,
		CancellationToken cancellationToken
	)
	{
		if (request == null)
		{
			throw new ArgumentNullException(nameof(request));
		}

		if (request.RequestUri == null)
		{
			throw new ArgumentException($"{nameof(HttpRequestMessage.RequestUri)} is null.", nameof(request));
		}

		// begin tracking
		var operation = telemetryTracker.TrackOperationBegin(getId);

		// send the HTTP request and capture the response.
		var result = await base.SendAsync(request, cancellationToken);

		// track telemetry
		telemetryTracker.TrackDependencyEnd(operation, request.Method, request.RequestUri, result.StatusCode, result.IsSuccessStatusCode);

		return result;
	}
}
