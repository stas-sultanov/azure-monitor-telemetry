// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents a contract for publishing telemetry to the service.
/// </summary>
public interface TelemetryPublisher
{
	/// <summary>
	/// Asynchronously publishes telemetry items to the service.
	/// </summary>
	/// <param name="telemetryItems">A read-only list of telemetry items to publish.</param>
	/// <param name="tags">A read-only list of tags to attach to each telemetry item.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task representing the asynchronous operation that returns a <see cref="TelemetryPublishResult"/>.</returns>
	public Task<TelemetryPublishResult> PublishAsync
	(
		IReadOnlyList<Telemetry> telemetryItems,
		IReadOnlyList<KeyValuePair<String, String>>? tags,
		CancellationToken cancellationToken
	);
}
