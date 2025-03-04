// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace System.Net.Http;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

[DebuggerNonUserCode]
[ExcludeFromCodeCoverage]
internal static class HttpContentExtensions
{
#pragma warning disable IDE0060 // Remove unused parameter
	public static Task<String> ReadAsStringAsync(this HttpContent httpContent, CancellationToken cancellationToken)
	{
		return httpContent.ReadAsStringAsync();
	}
}
