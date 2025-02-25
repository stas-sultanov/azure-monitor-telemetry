// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using System.Net;
using System.Net.Http;

internal sealed class HttpMessageHandlerMock : HttpMessageHandler
{
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent("OK")
		});
	}

	public Task<HttpResponseMessage> FakeSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		return SendAsync(request, cancellationToken);
	}
}
