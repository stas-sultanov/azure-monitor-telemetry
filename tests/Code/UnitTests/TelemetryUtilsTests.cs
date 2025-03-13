// Created by Stas Sultanov.
// Copyright � Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

using Azure.Monitor.Telemetry.Models;

/// <summary>
/// Tests for <see cref="TelemetryUtils"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
public sealed class TelemetryUtilsTests
{
	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureBlob()
	{
		// arrange
		var uri = new Uri("https://myaccount.blob.core.windows.net");

		// act
		var result = TelemetryUtils.DetectDependencyTypeFromHttpUri(uri);

		// assert
		Assert.AreEqual(DependencyTypes.AzureBlob, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureCosmosDB()
	{
		// arrange
		var uri = new Uri("https://myaccount.documents.azure.com");

		// act
		var result = TelemetryUtils.DetectDependencyTypeFromHttpUri(uri);

		// assert
		Assert.AreEqual(DependencyTypes.AzureCosmosDB, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureIotHub()
	{
		// arrange
		var uri = new Uri("https://myaccount.azure-devices.net");

		// act
		var result = TelemetryUtils.DetectDependencyTypeFromHttpUri(uri);

		// assert
		Assert.AreEqual(DependencyTypes.AzureIotHub, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureMonitor()
	{
		// arrange
		var uri = new Uri("https://myaccount.applicationinsights.azure.com");

		// act
		var result = TelemetryUtils.DetectDependencyTypeFromHttpUri(uri);

		// assert
		Assert.AreEqual(DependencyTypes.AzureMonitor, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureQueue()
	{
		// arrange
		var uri = new Uri("https://myaccount.queue.core.windows.net");

		// act
		var result = TelemetryUtils.DetectDependencyTypeFromHttpUri(uri);

		// assert
		Assert.AreEqual(DependencyTypes.AzureQueue, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureSearch()
	{
		// arrange
		var uri = new Uri("https://myaccount.search.windows.net");

		// act
		var result = TelemetryUtils.DetectDependencyTypeFromHttpUri(uri);

		// assert
		Assert.AreEqual(DependencyTypes.AzureSearch, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureServiceBus()
	{
		// arrange
		var uri = new Uri("https://myaccount.servicebus.windows.net");

		// act
		var result = TelemetryUtils.DetectDependencyTypeFromHttpUri(uri);

		// assert
		Assert.AreEqual(DependencyTypes.AzureServiceBus, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureTable()
	{
		// arrange
		var uri = new Uri("https://myaccount.table.core.windows.net");

		// act
		var result = TelemetryUtils.DetectDependencyTypeFromHttpUri(uri);

		// assert
		Assert.AreEqual(DependencyTypes.AzureTable, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnHttp()
	{
		// arrange
		var uri = new Uri("https://unknownuri.com");

		// act
		var result = TelemetryUtils.DetectDependencyTypeFromHttpUri(uri);

		// assert
		Assert.AreEqual(DependencyTypes.HTTP, result);
	}
}