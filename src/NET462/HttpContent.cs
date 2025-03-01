// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace System.Net.Http;

using System.Threading.Tasks;

internal static class HttpContentExtensions
{
#pragma warning disable IDE0060 // Remove unused parameter
	public static Task<String> ReadAsStringAsync(this HttpContent httpContet, CancellationToken cancellationToken)
	{
		return httpContet.ReadAsStringAsync();
	}
}
