// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.Mocks;

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Azure.Monitor.Telemetry.Publish;

internal sealed class HttpTelemetryPublisherMock : TelemetryPublisher
{
	#region fields

	public const String MockValidIngestEndpoint = @"https://dc.in.applicationinsights.azure.com/";

	public static readonly Uri MockValidIngestEndpointUri = new(MockValidIngestEndpoint);

	#endregion

	public List<Telemetry> Buffer { get; } = [];

	/// <inheritdoc/>
	public Task<TelemetryPublishResult> PublishAsync
	(
		IReadOnlyList<Telemetry> telemetryList,
		KeyValuePair<String, String>[]? tags,
		CancellationToken cancellationToken
	)
	{
		var time = DateTime.UtcNow;

		Buffer.AddRange(telemetryList);

		var publishResult = (TelemetryPublishResult)new HttpTelemetryPublishResult(telemetryList.Count, DateTime.UtcNow.Subtract(time), true, time, MockValidIngestEndpointUri, HttpStatusCode.OK, "OK");

		var result = Task.FromResult(publishResult);

		return result;
	}
}
