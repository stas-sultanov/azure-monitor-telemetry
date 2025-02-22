// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents a contract for publishing telemetry data to a monitoring service.
/// </summary>
public interface TelemetryPublisher
{
	/// <summary>
	/// Asynchronously publishes telemetry data with associated tags to the service.
	/// </summary>
	/// <param name="telemetryList">A read-only list of telemetry items to publish.</param>
	/// <param name="trackerTags">A list of tags to attach to each telemetry item.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task representing the asynchronous operation that returns a <see cref="TelemetryPublishResult"/>.</returns>
	public Task<TelemetryPublishResult> PublishAsync
	(
		IReadOnlyList<Telemetry> telemetryList,
		KeyValuePair<String, String>[] trackerTags,
		CancellationToken cancellationToken
	);
}
