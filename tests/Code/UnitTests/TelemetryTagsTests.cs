// Authored by Stas Sultanov
// Copyright © Stas Sultanov

namespace Azure.Monitor.Telemetry.Tests;

using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for <see cref="TelemetryClient"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class TelemetryTagsTests
{
	#region Methods: Tests

	[TestMethod]
	public void Constructor_ThrowsException()
	{
		_ = Assert.ThrowsExactly<ArgumentNullException>
		(
			() => _ = new TelemetryTags(null!)
		);
	}

	[TestMethod]
	public void Constructor_WithDictionary_ShouldInitialize()
	{
		// arrange
		var applicationVerValue = "1.1";
		var customKey = "ai.cloud.name";
		var customKeyValue = "value";
		var userIdValue = "100";

		var source = new Dictionary<String, String>
		{
			{ TelemetryTagKeys.ApplicationVer, applicationVerValue }, // supported key
			{ customKey, customKeyValue } // not supported key
		};

		// act
		var tags = new TelemetryTags(source)
		{
			UserId = userIdValue
		};

		var tagsAsDictionary = new Dictionary<String, String>(tags.ToArray());

		// assert
		Assert.AreEqual(3, tagsAsDictionary.Count);
		Assert.AreEqual(applicationVerValue, tagsAsDictionary[TelemetryTagKeys.ApplicationVer]);
		Assert.AreEqual(customKeyValue, tagsAsDictionary[customKey]);
		Assert.AreEqual(userIdValue, tagsAsDictionary[TelemetryTagKeys.UserId]);
	}

	[TestMethod]
	public void Property_IsEmpty_ShouldReturnTrueForEmptyCollection()
	{
		// Arrange
		var tags = new TelemetryTags();

		// Act
		var isEmpty = tags.IsEmpty();

		// Assert
		Assert.IsTrue(isEmpty, "IsEmpty should return true for an empty collection.");
	}

	[TestMethod]
	public void Property_IsEmpty_ShouldReturnFalseForNonEmptyCollection()
	{
		// Arrange
		var source = new Dictionary<String, String>
		{
			{ "key1", "value1" }
		};
		var tags = new TelemetryTags(source);

		// Act
		var isEmpty = tags.IsEmpty();

		// Assert
		Assert.IsFalse(isEmpty, "IsEmpty should return false for a non-empty collection.");
	}

	[TestMethod]
	public void Property_Empty_ShouldReturnEmptyInstance()
	{
		// Act
		var emptyTags = TelemetryTags.Empty;

		// Assert
		Assert.IsTrue(emptyTags.IsEmpty(), "The Empty property should return an empty instance.");
	}

	[TestMethod]
	public void Properties_ShouldBeAssignedCorrectly()
	{
		// arrange
		const String applicationVersion = "1.0.0";
		const String cloudRole = "Role";
		const String cloudRoleInstance = "RoleInstance";
		const String deviceId = "DeviceId";
		const String deviceLocale = "US";
		const String deviceModel = "Model123";
		const String deviceOEMName = "OEMName";
		const String deviceOSVersion = "10.0";
		const String deviceType = "Desktop";
		const String internalAgentVersion = "AgentVersion";
		const String internalNodeName = "NodeName";
		const String internalSdkVersion = "SdkVersion";
		const String locationCity = "City";
		const String locationCountry = "Country";
		const String locationIp = "192.168.1.1";
		const String locationProvince = "Province";
		const String operationCorrelationVector = "CorrelationVector";
		const String operationId = "OperationId";
		const String operationName = "OperationName";
		const String operationParentId = "ParentId";
		const String operationSyntheticSource = "SyntheticSource";
		const String sessionId = "SessionId";
		const String sessionIsFirst = "true";
		const String userAccountId = "AccountId";
		const String userAuthUserId = "AuthUserId";
		const String userId = "UserId";

		// act
		var tags = new TelemetryTags
		{
			ApplicationVer = applicationVersion,
			CloudRole = cloudRole,
			CloudRoleInstance = cloudRoleInstance,
			DeviceId = deviceId,
			DeviceLocale = deviceLocale,
			DeviceModel = deviceModel,
			DeviceOEMName = deviceOEMName,
			DeviceOSVersion = deviceOSVersion,
			DeviceType = deviceType,
			InternalAgentVersion = internalAgentVersion,
			InternalNodeName = internalNodeName,
			InternalSdkVersion = internalSdkVersion,
			LocationCity = locationCity,
			LocationCountry = locationCountry,
			LocationIp = locationIp,
			LocationProvince = locationProvince,
			OperationCorrelationVector = operationCorrelationVector,
			OperationId = operationId,
			OperationName = operationName,
			OperationParentId = operationParentId,
			OperationSyntheticSource = operationSyntheticSource,
			SessionId = sessionId,
			SessionIsFirst = sessionIsFirst,
			UserAccountId = userAccountId,
			UserAuthUserId = userAuthUserId,
			UserId = userId
		};

		// assert
		AssertHelper.PropertyEqualsTo(tags, o => o.ApplicationVer, applicationVersion);
		AssertHelper.PropertyEqualsTo(tags, o => o.CloudRole, cloudRole);
		AssertHelper.PropertyEqualsTo(tags, o => o.CloudRoleInstance, cloudRoleInstance);
		AssertHelper.PropertyEqualsTo(tags, o => o.DeviceId, deviceId);
		AssertHelper.PropertyEqualsTo(tags, o => o.DeviceLocale, deviceLocale);
		AssertHelper.PropertyEqualsTo(tags, o => o.DeviceModel, deviceModel);
		AssertHelper.PropertyEqualsTo(tags, o => o.DeviceOEMName, deviceOEMName);
		AssertHelper.PropertyEqualsTo(tags, o => o.DeviceOSVersion, deviceOSVersion);
		AssertHelper.PropertyEqualsTo(tags, o => o.DeviceType, deviceType);
		AssertHelper.PropertyEqualsTo(tags, o => o.InternalAgentVersion, internalAgentVersion);
		AssertHelper.PropertyEqualsTo(tags, o => o.InternalNodeName, internalNodeName);
		AssertHelper.PropertyEqualsTo(tags, o => o.InternalSdkVersion, internalSdkVersion);
		AssertHelper.PropertyEqualsTo(tags, o => o.LocationCity, locationCity);
		AssertHelper.PropertyEqualsTo(tags, o => o.LocationCountry, locationCountry);
		AssertHelper.PropertyEqualsTo(tags, o => o.LocationIp, locationIp);
		AssertHelper.PropertyEqualsTo(tags, o => o.LocationProvince, locationProvince);
		AssertHelper.PropertyEqualsTo(tags, o => o.OperationCorrelationVector, operationCorrelationVector);
		AssertHelper.PropertyEqualsTo(tags, o => o.OperationId, operationId);
		AssertHelper.PropertyEqualsTo(tags, o => o.OperationName, operationName);
		AssertHelper.PropertyEqualsTo(tags, o => o.OperationParentId, operationParentId);
		AssertHelper.PropertyEqualsTo(tags, o => o.OperationSyntheticSource, operationSyntheticSource);
		AssertHelper.PropertyEqualsTo(tags, o => o.SessionId, sessionId);
		AssertHelper.PropertyEqualsTo(tags, o => o.SessionIsFirst, sessionIsFirst);
		AssertHelper.PropertyEqualsTo(tags, o => o.UserAccountId, userAccountId);
		AssertHelper.PropertyEqualsTo(tags, o => o.UserAuthUserId, userAuthUserId);
		AssertHelper.PropertyEqualsTo(tags, o => o.UserId, userId);
	}

	#endregion
}
