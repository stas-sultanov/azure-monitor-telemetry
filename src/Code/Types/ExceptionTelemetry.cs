// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Represents telemetry of an exception that occurred in an application.
/// </summary>
/// <remarks>
/// This class is used to track and report exceptions in the application, including their stack traces
/// and other relevant details. The maximum length of the stack trace is limited to 32768 characters.
/// </remarks>
public sealed class ExceptionTelemetry : Telemetry
{
	private const Int32 MaxStackLength = 32768;

	#region Static Methods

	public static IReadOnlyList<ExceptionInfo> Convert(Exception exception, Int32 maxStackLength = MaxStackLength)
	{
		var result = new List<ExceptionInfo>();

		var outerId = 0;

		var currentException = exception;

		do
		{
			// get id
			var id = currentException.GetHashCode();

			// get stack trace
			var stackTrace = new System.Diagnostics.StackTrace(currentException, true);

			// get frames
			var frames = stackTrace.GetFrames();

			// calc number of frames to take
			var takeFramesCount = Math.Min(frames.Length, maxStackLength);

			var parsedStack = new ExceptionFrameInfo[takeFramesCount];

			for (var frameIndex = 0; frameIndex < takeFramesCount; frameIndex++)
			{
				var frame = frames[frameIndex];

				var methodInfo = frame.GetMethod();

				var method = methodInfo?.DeclaringType == null ? methodInfo?.Name: String.Concat(methodInfo.DeclaringType.FullName, ".", methodInfo.Name);

				var frameInfo = new ExceptionFrameInfo
				{
					Assembly = methodInfo?.Module.Assembly.FullName!,
					Level = frameIndex,
					Line = frame.GetFileLineNumber(),
					Method = method
				};

				parsedStack[frameIndex] = frameInfo;
			}

			var exceptionInfo = new ExceptionInfo()
			{
				HasFullStack = stackTrace.FrameCount < maxStackLength,
				Id = id,
				Message = currentException.Message.Replace("\r\n", " "),
				OuterId = outerId,
				ParsedStack = parsedStack,
				TypeName = currentException.GetType().FullName!
			};

			result.Add(exceptionInfo);

			outerId = id;

			currentException = currentException.InnerException;
		}
		while (currentException != null);

		return result;
	}

	#endregion

	#region Properties

	/// <summary>
	/// A read only list that represents execptions stack.
	/// </summary>
	public required IReadOnlyList<ExceptionInfo> Exceptions { get; init; }

	/// <summary>
	/// A read-only list of measurements.
	/// </summary>
	/// <remarks>
	/// Maximum key length: 150 characters.
	/// Is null by default.
	/// </remarks>
	public IReadOnlyList<KeyValuePair<String, Double>>? Measurements { get; init; }

	/// <inheritdoc/>
	public required TelemetryOperation Operation { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Properties { get; init; }

	/// <summary>
	/// The severity level.
	/// </summary>
	public SeverityLevel? SeverityLevel { get; init; }

	/// <inheritdoc/>
	public IReadOnlyList<KeyValuePair<String, String>>? Tags { get; init; }

	/// <summary>
	/// The UTC timestamp when the exception has occurred.
	/// </summary>
	public required DateTime Time { get; init; }

	#endregion
}
