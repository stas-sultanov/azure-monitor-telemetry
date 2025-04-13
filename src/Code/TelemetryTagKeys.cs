// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// A collection of well-known string constants used as keys for telemetry tags in Azure Monitor.
/// </summary>
public static class TelemetryTagKeys
{
	/// <summary>
	/// The version of the application.
	/// </summary>
	public const String ApplicationVer = @"ai.application.ver";

	/// <summary>
	/// The name of the role of the application within the solution.
	/// </summary>
	public const String CloudRole = @"ai.cloud.role";

	/// <summary>
	/// The name of the instance where the application is running.
	/// </summary>
	public const String CloudRoleInstance = @"ai.cloud.roleInstance";

	/// <summary>
	/// The unique identifier for the client device.
	/// </summary>
	public const String DeviceId = @"ai.device.id";

	/// <summary>
	/// The locale of the device in the format [language]-[REGION], following RFC 5646.
	/// </summary>
	public const String DeviceLocale = @"ai.device.locale";

	/// <summary>
	/// The model of the device.
	/// </summary>
	public const String DeviceModel = @"ai.device.model";

	/// <summary>
	/// The OEM name of the device.
	/// </summary>
	public const String DeviceOEMName = @"ai.device.oemName";

	/// <summary>
	/// The operating system name and version of the device.
	/// </summary>
	public const String DeviceOSVersion = @"ai.device.osVersion";

	/// <summary>
	/// The type of the device.
	/// </summary>
	public const String DeviceType = @"ai.device.type";

	/// <summary>
	/// The version of the StatusMonitor agent installed on the machine, if applicable.
	/// </summary>
	public const String InternalAgentVersion = @"ai.internal.agentVersion";

	/// <summary>
	/// The name of the node used for billing purposes.
	/// </summary>
	public const String InternalNodeName = @"ai.internal.nodeName";

	/// <summary>
	/// The version of the SDK.
	/// </summary>
	public const String InternalSdkVersion = @"ai.internal.sdkVersion";

	/// <summary>
	/// The city where the client device is located.
	/// </summary>
	public const String LocationCity = @"ai.location.city";

	/// <summary>
	/// The country where the client device is located.
	/// </summary>
	public const String LocationCountry = @"ai.location.country";

	/// <summary>
	/// The IP address of the client device.
	/// </summary>
	public const String LocationIp = @"ai.location.ip";

	/// <summary>
	/// The province or state where the client device is located.
	/// </summary>
	public const String LocationProvince = @"ai.location.province";

	/// <summary>
	/// The lightweight vector clock used to identify and order related events across clients and services.
	/// </summary>
	public const String OperationCorrelationVector = @"ai.operation.correlationVector";

	/// <summary>
	/// The unique identifier for the operation instance.
	/// </summary>
	public const String OperationId = @"ai.operation.id";

	/// <summary>
	/// The name of the operation.
	/// </summary>
	public const String OperationName = @"ai.operation.name";

	/// <summary>
	/// The unique identifier of the parent operation, used to establish a hierarchy of operations.
	/// </summary>
	public const String OperationParentId = @"ai.operation.parentId";

	/// <summary>
	/// The name of the synthetic source.
	/// </summary>
	public const String OperationSyntheticSource = @"ai.operation.syntheticSource";

	/// <summary>
	/// The unique identifier of the session.
	/// </summary>
	public const String SessionId = @"ai.session.id";

	/// <summary>
	/// The flag indicating whether the session identified by <see cref="SessionId"/> is the first session for the user.
	/// </summary>
	public const String SessionIsFirst = @"ai.session.isFirst";

	/// <summary>
	/// The unique identifier of the user account.
	/// </summary>
	public const String UserAccountId = @"ai.user.accountId";

	/// <summary>
	/// The unique identifier of the authenticated user.
	/// </summary>
	public const String UserAuthUserId = @"ai.user.authUserId";

	/// <summary>
	/// The unique identifier of the anonymous user.
	/// </summary>
	public const String UserId = @"ai.user.id";
}
