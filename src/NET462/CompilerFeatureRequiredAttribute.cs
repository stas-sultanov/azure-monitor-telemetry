// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace System.Runtime.CompilerServices;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Indicates that compiler support for a particular feature is required for the location where this attribute is applied.
/// </summary>
/// <remarks>
/// Required to use required feature in .NET 4.6.2
/// </remarks>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
[DebuggerNonUserCode]
[ExcludeFromCodeCoverage]
internal sealed class CompilerFeatureRequiredAttribute(String featureName) : Attribute
{
	/// <summary>
	/// The name of the compiler feature.
	/// </summary>
	public String FeatureName { get; } = featureName;

	/// <summary>
	/// If true, the compiler can choose to allow access to the location where this attribute is applied if it does not understand <see cref="FeatureName"/>.
	/// </summary>
	public Boolean IsOptional { get; init; }

	/// <summary>
	/// The <see cref="FeatureName"/> used for the ref structs C# feature.
	/// </summary>
	public const String RefStructs = nameof(RefStructs);

	/// <summary>
	/// The <see cref="FeatureName"/> used for the required members C# feature.
	/// </summary>
	public const String RequiredMembers = nameof(RequiredMembers);
}
