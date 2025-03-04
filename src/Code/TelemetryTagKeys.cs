// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry;

/// <summary>
/// Provides a set of constant string keys used for telemetry tagging in Azure Monitor.
/// </summary>
public static class TelemetryTagKeys
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public const String ApplicationVersion = @"ai.application.ver";
	public const String CloudRole = @"ai.cloud.role";
	public const String CloudRoleInstance = @"ai.cloud.roleInstance";
	public const String DeviceId = @"ai.device.id";
	public const String DeviceLocale = @"ai.device.locale";
	public const String DeviceModel = @"ai.device.model";
	public const String DeviceOEMName = @"ai.device.oemName";
	public const String DeviceOSVersion = @"ai.device.osVersion";
	public const String DeviceType = @"ai.device.type";
	public const String InternalAgentVersion = @"ai.internal.agentVersion";
	public const String InternalNodeName = @"ai.internal.nodeName";
	public const String InternalSdkVersion = @"ai.internal.sdkVersion";
	public const String LocationCity = @"ai.location.city";
	public const String LocationCountry = @"ai.location.country";
	public const String LocationIp = @"ai.location.ip";
	public const String LocationProvince = @"ai.location.province";
	public const String OperationCorrelationVector = @"ai.operation.correlationVector";
	public const String OperationId = @"ai.operation.id";
	public const String OperationName = @"ai.operation.name";
	public const String OperationParentId = @"ai.operation.parentId";
	public const String OperationSyntheticSource = @"ai.operation.syntheticSource";
	public const String SessionId = @"ai.session.id";
	public const String SessionIsFirst = @"ai.session.isFirst";
	public const String UserAccountId = @"ai.user.accountId";
	public const String UserAuthUserId = @"ai.user.authUserId";
	public const String UserId = @"ai.user.id";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
