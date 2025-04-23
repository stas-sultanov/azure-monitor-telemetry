// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace System.Runtime.CompilerServices;

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// </summary>
/// <remarks>
/// This metadata class is required to use property init feature in .NET 4.6.2
/// </remarks>
[DebuggerNonUserCode]
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
internal static class IsExternalInit
{
}
