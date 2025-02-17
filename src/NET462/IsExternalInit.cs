// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace System.Runtime.CompilerServices;

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// This metadata class is required to use property init feature in .NET 4.6.2
/// </summary>
[DebuggerNonUserCode]
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
public static class IsExternalInit { }
