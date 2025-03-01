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
	public static String GetTelemetryId()
	{
		return ActivitySpanId.CreateRandom().ToString();
	}

	public static async Task SimulateAvailabilityAsync
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

	public static async Task SimulateDependencyAsync
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
}