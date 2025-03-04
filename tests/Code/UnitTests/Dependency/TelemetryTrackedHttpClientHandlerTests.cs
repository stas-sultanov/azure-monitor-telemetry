// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System.Net.Http;

using Azure.Monitor.Telemetry.Dependency;
using Azure.Monitor.Telemetry.Mocks;
using Azure.Monitor.Telemetry.Types;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for <see cref="TelemetryTrackedHttpClientHandler"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class TelemetryTrackedHttpClientHandlerTests : IDisposable
{
	#region Fields

	private readonly HttpTelemetryPublisherMock telemetryPublisher;
	private readonly TelemetryClient telemetryClient;
	private readonly TelemetryTrackedHttpClientHandler handler;
	private readonly HttpClient httpClient;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of <see cref="TelemetryTrackedHttpClientHandler"/> class.
	/// </summary>
	public TelemetryTrackedHttpClientHandlerTests()
	{
		telemetryPublisher = new();
		telemetryClient = new(telemetryPublisher);
		handler = new TelemetryTrackedHttpClientHandler(telemetryClient, () => "test-id");
		httpClient = new HttpClient(handler);
	}

	#endregion

	public void Dispose()
	{
		httpClient.Dispose();
	}

	#region Methods: Tests

	[TestMethod]
	public async Task SendAsync_TracksTelemetry()
	{
		// arrange
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
		// act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		var argumentNullException = Assert.ThrowsExactly<ArgumentNullException>(() => httpClient.Send(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.AreEqual("request", argumentNullException.ParamName);

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		var request = new HttpRequestMessage(HttpMethod.Get, (Uri) null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

		var argumentException = Assert.ThrowsExactly<InvalidOperationException>(() => httpClient.Send(request));

		//Assert.AreEqual("request", argumentException.ParamName);
	}

	#endregion
}