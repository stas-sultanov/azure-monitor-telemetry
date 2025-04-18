// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.Telemetry;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a helper to work with well-known telemetry tags supported by Azure Monitor.
/// </summary>
public sealed record TelemetryTags
{
	#region Static

	/// <summary>
	/// The empty instance of <see cref="TelemetryTags"/>.
	/// </summary>
	public static TelemetryTags Empty { get; } = new();

	#endregion

	#region Fields

	private readonly Dictionary<String, String> collection;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="TelemetryTags"/>.
	/// </summary>
	public TelemetryTags()
	{
		collection = [];
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TelemetryTags"/> by cloning the <paramref name="source"/>.
	/// </summary>
	/// <param name="source">The collection of tags.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
	public TelemetryTags
	(
		IReadOnlyDictionary<String, String> source
	)
	{
		if (source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		collection = new Dictionary<String, String>(source.Count);

		foreach (var item in source)
		{
			collection[item.Key] = item.Value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TelemetryTags"/> by cloning the <paramref name="source"/>.
	/// </summary>
	/// <param name="source">The instance of <see cref="TelemetryTags"/> to copy.</param>
	private TelemetryTags
	(
		TelemetryTags source
	)
	{
		collection = new Dictionary<String, String>(source.collection);
	}

	#endregion

	#region Properties

	/// <summary>
	/// The version of the application.
	/// </summary>
	/// <remarks>Maximum key length: 1024 characters.</remarks>
	public String? ApplicationVer
	{
		get => collection.TryGetValue(TelemetryTagKeys.ApplicationVer, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.ApplicationVer] = value;
			}
		}
	}

	/// <summary>
	/// The name of the role of the application within the solution.
	/// </summary>
	/// <remarks>Maximum key length: 256 characters.</remarks>
	public String? CloudRole
	{
		get => collection.TryGetValue(TelemetryTagKeys.CloudRole, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.CloudRole] = value;
			}
		}
	}

	/// <summary>
	/// The name of the instance where the application is running.
	/// </summary>
	/// <remarks>Maximum key length: 256 characters.</remarks>
	public String? CloudRoleInstance
	{
		get => collection.TryGetValue(TelemetryTagKeys.CloudRoleInstance, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.CloudRoleInstance] = value;
			}
		}
	}

	/// <summary>
	/// The unique identifier for the client device.
	/// </summary>
	/// <remarks>Maximum key length: 1024 characters.</remarks>
	public String? DeviceId
	{
		get => collection.TryGetValue(TelemetryTagKeys.DeviceId, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.DeviceId] = value;
			}
		}
	}

	/// <summary>
	/// The locale of the device in the format [language]-[REGION], following RFC 5646.
	/// </summary>
	/// <remarks>Maximum key length: 64 characters.</remarks>
	public String? DeviceLocale
	{
		get => collection.TryGetValue(TelemetryTagKeys.DeviceLocale, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.DeviceLocale] = value;
			}
		}
	}

	/// <summary>
	/// The model of the device.
	/// </summary>
	/// <remarks>Maximum key length: 256 characters.</remarks>
	public String? DeviceModel
	{
		get => collection.TryGetValue(TelemetryTagKeys.DeviceModel, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.DeviceModel] = value;
			}
		}
	}

	/// <summary>
	/// The OEM name of the device.
	/// </summary>
	/// <remarks>Maximum key length: 256 characters.</remarks>
	public String? DeviceOEMName
	{
		get => collection.TryGetValue(TelemetryTagKeys.DeviceOEMName, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.DeviceOEMName] = value;
			}
		}
	}

	/// <summary>
	/// The operating system name and version of the device.
	/// </summary>
	/// <remarks>Maximum key length: 256 characters.</remarks>
	public String? DeviceOSVersion
	{
		get => collection.TryGetValue(TelemetryTagKeys.DeviceOSVersion, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.DeviceOSVersion] = value;
			}
		}
	}

	/// <summary>
	/// The type of the device.
	/// </summary>
	/// <remarks>Maximum key length: 64 characters.</remarks>
	public String? DeviceType
	{
		get => collection.TryGetValue(TelemetryTagKeys.DeviceType, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.DeviceType] = value;
			}
		}
	}

	/// <summary>
	/// The version of the StatusMonitor agent installed on the machine, if applicable.
	/// </summary>
	/// <remarks>Maximum key length: 64 characters.</remarks>
	public String? InternalAgentVersion
	{
		get => collection.TryGetValue(TelemetryTagKeys.InternalAgentVersion, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.InternalAgentVersion] = value;
			}
		}
	}

	/// <summary>
	/// The name of the node used for billing purposes.
	/// </summary>
	/// <remarks>Maximum key length: 256 characters.</remarks>
	public String? InternalNodeName
	{
		get => collection.TryGetValue(TelemetryTagKeys.InternalNodeName, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.InternalNodeName] = value;
			}
		}
	}

	/// <summary>
	/// The version of the SDK.
	/// </summary>
	/// <remarks>Maximum key length: 64 characters.</remarks>
	public String? InternalSdkVersion
	{
		get => collection.TryGetValue(TelemetryTagKeys.InternalSdkVersion, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.InternalSdkVersion] = value;
			}
		}
	}

	/// <summary>
	/// The city where the client device is located.
	/// </summary>
	/// <remarks>Maximum key length: 256 characters.</remarks>
	public String? LocationCity
	{
		get => collection.TryGetValue(TelemetryTagKeys.LocationCity, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.LocationCity] = value;
			}
		}
	}

	/// <summary>
	/// The country where the client device is located.
	/// </summary>
	/// <remarks>Maximum key length: 256 characters.</remarks>
	public String? LocationCountry
	{
		get => collection.TryGetValue(TelemetryTagKeys.LocationCountry, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.LocationCountry] = value;
			}
		}
	}

	/// <summary>
	/// The IP address of the client device.
	/// </summary>
	/// <remarks>Maximum key length: 46 characters.</remarks>
	public String? LocationIp
	{
		get => collection.TryGetValue(TelemetryTagKeys.LocationIp, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.LocationIp] = value;
			}
		}
	}

	/// <summary>
	/// The province or state where the client device is located.
	/// </summary>
	/// <remarks>Maximum key length: 256 characters.</remarks>
	public String? LocationProvince
	{
		get => collection.TryGetValue(TelemetryTagKeys.LocationProvince, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.LocationProvince] = value;
			}
		}
	}

	/// <summary>
	/// The lightweight vector clock used to identify and order related events across clients and services.
	/// </summary>
	/// <remarks>Maximum key length: 64 characters.</remarks>
	public String? OperationCorrelationVector
	{
		get => collection.TryGetValue(TelemetryTagKeys.OperationCorrelationVector, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.OperationCorrelationVector] = value;
			}
		}
	}

	/// <summary>
	/// The unique identifier for the operation instance.
	/// </summary>
	/// <remarks>Maximum key length: 128 characters.</remarks>
	public String? OperationId
	{
		get => collection.TryGetValue(TelemetryTagKeys.OperationId, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.OperationId] = value;
			}
		}
	}

	/// <summary>
	/// The name of the operation.
	/// </summary>
	/// <remarks>Maximum key length: 1024 characters.</remarks>
	public String? OperationName
	{
		get => collection.TryGetValue(TelemetryTagKeys.OperationName, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.OperationName] = value;
			}
		}
	}

	/// <summary>
	/// The unique identifier of the parent operation, used to establish a hierarchy of operations.
	/// </summary>
	/// <remarks>Maximum key length: 128 characters.</remarks>
	public String? OperationParentId
	{
		get => collection.TryGetValue(TelemetryTagKeys.OperationParentId, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.OperationParentId] = value;
			}
		}
	}

	/// <summary>
	/// The name of the synthetic source.
	/// </summary>
	/// <remarks>Maximum key length: 1024 characters.</remarks>
	public String? OperationSyntheticSource
	{
		get => collection.TryGetValue(TelemetryTagKeys.OperationSyntheticSource, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.OperationSyntheticSource] = value;
			}
		}
	}

	/// <summary>
	/// The unique identifier of the session.
	/// </summary>
	/// <remarks>Maximum key length: 64 characters.</remarks>
	public String? SessionId
	{
		get => collection.TryGetValue(TelemetryTagKeys.SessionId, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.SessionId] = value;
			}
		}
	}

	/// <summary>
	/// The flag indicating whether the session identified by <see cref="SessionId"/> is the first session for the user.
	/// </summary>
	/// <remarks>Maximum key length: 5 characters.</remarks>
	public String? SessionIsFirst
	{
		get => collection.TryGetValue(TelemetryTagKeys.SessionIsFirst, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.SessionIsFirst] = value;
			}
		}
	}

	/// <summary>
	/// The unique identifier of the user account.
	/// </summary>
	/// <remarks>Maximum key length: 1024 characters.</remarks>
	public String? UserAccountId
	{
		get => collection.TryGetValue(TelemetryTagKeys.UserAccountId, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.UserAccountId] = value;
			}
		}
	}

	/// <summary>
	/// The unique identifier of the authenticated user.
	/// </summary>
	/// <remarks>Maximum key length: 1024 characters.</remarks>
	public String? UserAuthUserId
	{
		get => collection.TryGetValue(TelemetryTagKeys.UserAuthUserId, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.UserAuthUserId] = value;
			}
		}
	}

	/// <summary>
	/// The unique identifier of the anonymous user.
	/// </summary>
	/// <remarks>Maximum key length: 128 characters.</remarks>
	public String? UserId
	{
		get => collection.TryGetValue(TelemetryTagKeys.UserId, out var value) ? value : null;

		init
		{
			if (value != null)
			{
				collection[TelemetryTagKeys.UserId] = value;
			}
		}
	}

	#endregion

	#region Methods

	/// <summary>
	/// Creates an array from the instance.
	/// </summary>
	/// <returns>An array that contains the elements from the instance.</returns>
	public KeyValuePair<String, String>[] ToArray()
	{
		return [.. collection];
	}

	/// <summary>
	/// Gets a value that indicates whether the instance contains no values.
	/// </summary>
	/// <returns><c>true</c> if the instance contains no values; otherwise, <c>false</c>.</returns>
	public Boolean IsEmpty()
	{
		return collection.Count == 0;
	}

	#endregion
}
