// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

namespace Azure.Monitor.Telemetry.UnitTests;

/// <summary>
/// Tests for <see cref="DependencyType"/> class.
/// </summary>
[TestCategory("UnitTests")]
[TestClass]
sealed public class DependencyTypeTests
{
	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureBlob()
	{
		// arrange
		var uri = new Uri("https://myaccount.blob.core.windows.net");

		// act
		var result = DependencyType.DetectTypeFromHttp(uri);

		// assert
		Assert.AreEqual(DependencyType.AzureBlob, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureCosmosDB()
	{
		// arrange
		var uri = new Uri("https://myaccount.documents.azure.com");

		// act
		var result = DependencyType.DetectTypeFromHttp(uri);

		// assert
		Assert.AreEqual(DependencyType.AzureCosmosDB, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureIotHub()
	{
		// arrange
		var uri = new Uri("https://myaccount.azure-devices.net");

		// act
		var result = DependencyType.DetectTypeFromHttp(uri);

		// assert
		Assert.AreEqual(DependencyType.AzureIotHub, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureMonitor()
	{
		// arrange
		var uri = new Uri("https://myaccount.applicationinsights.azure.com");

		// act
		var result = DependencyType.DetectTypeFromHttp(uri);

		// assert
		Assert.AreEqual(DependencyType.AzureMonitor, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureQueue()
	{
		// arrange
		var uri = new Uri("https://myaccount.queue.core.windows.net");

		// act
		var result = DependencyType.DetectTypeFromHttp(uri);

		// assert
		Assert.AreEqual(DependencyType.AzureQueue, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureSearch()
	{
		// arrange
		var uri = new Uri("https://myaccount.search.windows.net");

		// act
		var result = DependencyType.DetectTypeFromHttp(uri);

		// assert
		Assert.AreEqual(DependencyType.AzureSearch, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureServiceBus()
	{
		// arrange
		var uri = new Uri("https://myaccount.servicebus.windows.net");

		// act
		var result = DependencyType.DetectTypeFromHttp(uri);

		// assert
		Assert.AreEqual(DependencyType.AzureServiceBus, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnAzureTable()
	{
		// arrange
		var uri = new Uri("https://myaccount.table.core.windows.net");

		// act
		var result = DependencyType.DetectTypeFromHttp(uri);

		// assert
		Assert.AreEqual(DependencyType.AzureTable, result);
	}

	[TestMethod]
	public void Method_DetectTypeFromHttp_ShouldReturnHttp()
	{
		// arrange
		var uri = new Uri("https://unknownuri.com");

		// act
		var result = DependencyType.DetectTypeFromHttp(uri);

		// assert
		Assert.AreEqual(DependencyType.HTTP, result);
	}
}