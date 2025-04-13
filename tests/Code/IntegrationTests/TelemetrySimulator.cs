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

	public static async Task SimulateAvailabilityTestCallAsync
	(
		TelemetryClient telemetryClient,
		String name,
		String? runLocation,
		Func<String, CancellationToken, Task<Boolean>> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin activity scope
		telemetryClient.ActivityScopeBegin(GetActivityId, out var time, out var timestamp, out var activityId, out var context);

		// execute subsequent
		var success = await subsequent(activityId, cancellationToken);

		// end activity scope
		telemetryClient.ActivityScopeEnd(context, timestamp, out var duration);

		// track telemetry
		var message = success ? "Passed" : "Failed";

		telemetryClient.TrackAvailability(time, duration, activityId, name, message, success, runLocation);
	}

	public static async Task SimulateHttpDependencyCallAsync
	(
		TelemetryClient telemetryClient,
		HttpMethod httpMethod,
		Uri url,
		Func<String, CancellationToken, Task<Boolean>> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin activity scope
		telemetryClient.ActivityScopeBegin(GetActivityId, out var time, out var timestamp, out var activityId, out var context);

		// execute subsequent
		var success = await subsequent(activityId, cancellationToken);

		// end activity scope
		telemetryClient.ActivityScopeEnd(context, timestamp, out var duration);

		// track telemetry
		var statusCode = success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;

		telemetryClient.TrackDependencyHttp(time, duration, activityId, httpMethod, url, statusCode, (Int32) statusCode < 399);
	}

	public static async Task SimulatePageViewAsync
	(
		TelemetryClient telemetryClient,
		String pageName,
		Uri pageUrl,
		Func<String, CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin activity scope
		telemetryClient.ActivityScopeBegin(GetActivityId, out var time, out var timestamp, out var activityId, out var context);

		// execute subsequent
		await subsequent(activityId, cancellationToken);

		// end activity scope
		telemetryClient.ActivityScopeEnd(context, timestamp, out var duration);

		// track telemetry
		telemetryClient.TrackPageView(time, duration, activityId, pageName, pageUrl);
	}

	public static async Task SimulateRequestProcessingAsync
	(
		TelemetryClient telemetryClient,
		Uri url,
		String responseCode,
		Boolean success,
		Func<String, CancellationToken, Task> subsequent,
		CancellationToken cancellationToken
	)
	{
		// begin activity scope
		telemetryClient.ActivityScopeBegin(GetActivityId, out var time, out var timestamp, out var activityId, out var context);

		// execute subsequent
		await subsequent(activityId, cancellationToken);

		// end activity scope
		telemetryClient.ActivityScopeEnd(context, timestamp, out var duration);

		// track telemetry
		telemetryClient.TrackRequest(time, duration, activityId, url, responseCode, success);
	}

	#endregion
}
