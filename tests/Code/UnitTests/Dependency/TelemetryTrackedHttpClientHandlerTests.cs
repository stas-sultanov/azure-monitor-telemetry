// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.Telemetry.Tests;

using System.Net.Http;

using Azure.Monitor.Telemetry.Dependency;
using Azure.Monitor.Telemetry.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for <see cref="TelemetryTrackedHttpClientHandler"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class TelemetryTrackedHttpClientHandlerTests
{
	#region Methods: Tests

	[TestMethod]
	public async Task SendAsync_TracksTelemetry()
	{
		// arrange
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryClient = new TelemetryClient(telemetryPublisher);
		using var handler = new TelemetryTrackedHttpClientHandler(telemetryClient, TelemetryFactory.GetActivityId);
		using var httpClient = new HttpClient(handler);

		var request = new HttpRequestMessage(HttpMethod.Get, "https://google.com");

		// act
		_ = await httpClient.SendAsync(request, CancellationToken.None);

		_ = await telemetryClient.PublishAsync(CancellationToken.None);

		// assert
		Assert.AreEqual(1, telemetryPublisher.Buffer.Count, "Items Count");

		var telemetry = telemetryPublisher.Buffer.Dequeue();

		Assert.IsInstanceOfType<DependencyTelemetry>(telemetry);
	}

	[TestMethod]
	public void SendAsync_ThrowsException()
	{
		var telemetryPublisher = new HttpTelemetryPublisherMock();
		var telemetryClient = new TelemetryClient(telemetryPublisher);
		using var handler = new TelemetryTrackedHttpClientHandler(telemetryClient, TelemetryFactory.GetActivityId);
		using var httpClient = new HttpClient(handler);

		// act
		var argumentNullException = Assert.ThrowsExactly<ArgumentNullException>(() => httpClient.Send(null!));

		Assert.AreEqual("request", argumentNullException.ParamName);

		var request = new HttpRequestMessage(HttpMethod.Get, (Uri) null!);

		var argumentException = Assert.ThrowsExactly<InvalidOperationException>(() => httpClient.Send(request));
	}

	#endregion
}
