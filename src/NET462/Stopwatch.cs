// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace System.Diagnostics;

using System;

using NativeStopwatch = System.Diagnostics.Stopwatch;

public static class Stopwatch2
{
	/// <summary>Gets the elapsed time between two timestamps retrieved using <see cref="GetTimestamp"/>.</summary>
	/// <param name="startingTimestamp">The timestamp marking the beginning of the time period.</param>
	/// <param name="endingTimestamp">The timestamp marking the end of the time period.</param>
	/// <returns>A <see cref="TimeSpan"/> for the elapsed time between the starting and ending timestamps.</returns>
	public static TimeSpan GetElapsedTime(Int64 startingTimestamp, Int64 endingTimestamp)
	{
		// calculate duration
		TimeSpan result = new ((endingTimestamp - startingTimestamp) * TimeSpan.TicksPerSecond / NativeStopwatch.Frequency);

		return result;
	}
}
