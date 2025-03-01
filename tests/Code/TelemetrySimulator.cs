// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Tests;

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;

using Azure.Monitor.Telemetry;

internal static class TelemetrySimulator
{
	public static String GetActivityId()
	{
		return ActivitySpanId.CreateRandom().ToString();
	}

	public static async Task SimulateAvailabilityAsync
	(
		TelemetryTracker telemetryTracker,
		String name,
		String message,
		Boolean success,
		String? runLocation,
		Func<CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin activity scope
		telemetryTracker.ActivityScopeBegin(GetActivityId, out var time, out var timestamp, out var id, out var operation);

		// execute subsequent
		await subsequent(cancellationToken);

		// end activity scope
		var duration = telemetryTracker.ActivityScopeEnd(timestamp, operation);

		// track telemetry
		telemetryTracker.TrackAvailability(time, duration, id, name, message, success, runLocation);
	}

	public static async Task SimulateDependencyAsync
	(
		TelemetryTracker telemetryTracker,
		HttpMethod httpMethod,
		Uri url,
		HttpStatusCode statusCode,
		Func<CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin activity scope
		telemetryTracker.ActivityScopeBegin(GetActivityId, out var time, out var timestamp, out var id, out var operation);

		// execute subsequent
		await subsequent(cancellationToken);

		// end activity scope
		var duration = telemetryTracker.ActivityScopeEnd(timestamp, operation);

		// track telemetry
		telemetryTracker.TrackDependency(time, duration, id, httpMethod, url, statusCode, (Int32) statusCode < 399);
	}

	public static async Task SimulatePageViewAsync
	(
		TelemetryTracker telemetryTracker,
		String pageName,
		Uri pageUrl,
		Func<CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin operation
		telemetryTracker.ActivityScopeBegin(GetActivityId, out var time, out var timestamp, out var id, out var operation);

		// execute subsequent
		await subsequent(cancellationToken);

		// end activity scope
		var duration = telemetryTracker.ActivityScopeEnd(timestamp, operation);

		// track telemetry
		telemetryTracker.TrackPageView(time, duration, id, pageName, pageUrl);
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
		telemetryTracker.ActivityScopeBegin(GetActivityId, out var time, out var timestamp, out var id, out var operation);

		// execute subsequent
		await subsequent(cancellationToken);

		// end activity scope
		var duration = telemetryTracker.ActivityScopeEnd(timestamp, operation);

		// track telemetry
		telemetryTracker.TrackRequest(time, duration, id, url, responseCode, success);
	}
}