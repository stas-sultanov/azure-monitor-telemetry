// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Tests;

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;

using Azure.Monitor.Telemetry;

/// <summary>
/// Provides methods to simulate telemetry events.
/// </summary>
internal static class TelemetrySimulator
{
	#region Methods

	public static String GetActivityId()
	{
		return ActivitySpanId.CreateRandom().ToString();
	}

	public static async Task SimulateAvailabilityAsync
	(
		TelemetryClient telemetryClient,
		String name,
		String message,
		Boolean success,
		String? runLocation,
		Func<CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin activity scope
		telemetryClient.ActivityScopeBegin(GetActivityId, out var time, out var timestamp, out var id, out var context);

		// execute subsequent
		await subsequent(cancellationToken);

		// end activity scope
		telemetryClient.ActivityScopeEnd(context, timestamp, out var duration);

		// track telemetry
		telemetryClient.TrackAvailability(time, duration, id, name, message, success, runLocation);
	}

	public static async Task SimulateDependencyAsync
	(
		TelemetryClient telemetryClient,
		HttpMethod httpMethod,
		Uri url,
		HttpStatusCode statusCode,
		Func<CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin activity scope
		telemetryClient.ActivityScopeBegin(GetActivityId, out var time, out var timestamp, out var id, out var context);

		// execute subsequent
		await subsequent(cancellationToken);

		// end activity scope
		telemetryClient.ActivityScopeEnd(context, timestamp, out var duration);

		// track telemetry
		telemetryClient.TrackDependencyHttp(time, duration, id, httpMethod, url, statusCode, (Int32) statusCode < 399);
	}

	public static async Task SimulatePageViewAsync
	(
		TelemetryClient telemetryClient,
		String pageName,
		Uri pageUrl,
		Func<CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin activity scope
		telemetryClient.ActivityScopeBegin(GetActivityId, out var time, out var timestamp, out var id, out var context);

		// execute subsequent
		await subsequent(cancellationToken);

		// end activity scope
		telemetryClient.ActivityScopeEnd(context, timestamp, out var duration);

		// track telemetry
		telemetryClient.TrackPageView(time, duration, id, pageName, pageUrl);
	}

	public static async Task SimulateRequestAsync
	(
		TelemetryClient telemetryClient,
		Uri url,
		String responseCode,
		Boolean success,
		Func<CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin activity scope
		telemetryClient.ActivityScopeBegin(GetActivityId, out var time, out var timestamp, out var id, out var context);

		// execute subsequent
		await subsequent(cancellationToken);

		// end activity scope
		telemetryClient.ActivityScopeEnd(context, timestamp, out var duration);

		// track telemetry
		telemetryClient.TrackRequest(time, duration, id, url, responseCode, success);
	}

	#endregion
}
