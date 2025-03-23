// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace System.Runtime.CompilerServices;

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies that a type has required members or that a member is required.
/// </summary>
/// <remarks>
/// Required to use required feature in .NET 4.6.2
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
[DebuggerNonUserCode]
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
internal sealed class RequiredMemberAttribute : Attribute
{
}
