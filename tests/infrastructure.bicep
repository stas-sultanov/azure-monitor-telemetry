// Created by Stas Sultanov.
// Copyright © Stas Sultanov.

/* parameters */

@description('Base name to use for names of resources.')
@minLength(4)
@maxLength(20)
param baseName string

@description('Location to deploy the resources.')
param location string = resourceGroup().location

@description('Resources tags.')
param tags object

/* resources */

// https://learn.microsoft.com/azure/templates/microsoft.insights/components
@description('Application insights without auth.')
resource Insights_components_AuthOff 'Microsoft.Insights/components@2020-02-02' = {
	kind: 'web'
	location: location
	name: '${baseName}off'
	properties: {
		Application_Type: 'web'
		DisableLocalAuth: false
		WorkspaceResourceId: OperationalInsights_workspaces_Default.id
	}
	tags: union(
		tags,
		{
			'hidden-title': 'auth off'
		}
	)
}

// https://learn.microsoft.com/azure/templates/microsoft.insights/components
@description('Application insights with auth.')
resource Insights_components_AuthOn 'Microsoft.Insights/components@2020-02-02' = {
	kind: 'web'
	location: location
	name: '${baseName}on'
	properties: {
		Application_Type: 'web'
		DisableLocalAuth: true
		WorkspaceResourceId: OperationalInsights_workspaces_Default.id
	}
	tags: union(
		tags,
		{
			'hidden-title': 'auth on'
		}
	)
}

// https://learn.microsoft.com/azure/templates/microsoft.authorization/roleassignments
@description('Application insights with auth authorization.')
resource Insights_components_AuthOn_Authorization_roleAssignments_ 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
	name: guid(
		Insights_components_AuthOn.id,
		deployer().objectId,
		'Monitoring Metrics Publisher'
	)
	properties: {
		description: 'Authorize deployer'
		principalId: deployer().objectId
		roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '3913510d-42f4-4e42-8a64-420c390055eb')
	}
	scope: Insights_components_AuthOn
}

// https://learn.microsoft.com/azure/templates/microsoft.operationalinsights/workspaces
resource OperationalInsights_workspaces_Default 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
	location: location
	name: baseName
	properties: {
		features: {
			disableLocalAuth: true
		}
		retentionInDays: 30
		sku: {
			name: 'PerGB2018'
		}
	}
	tags: union(
		tags,
		{
			'hidden-title': 'default'
		}
	)
}

// https://learn.microsoft.com/azure/templates/microsoft.resources/tags
@description('Tags on resource group')
resource Resources_tags_Default 'Microsoft.Resources/tags@2024-03-01' = {
	name: 'default'
	properties: {
		tags: tags
	}
}

// https://learn.microsoft.com/azure/templates/microsoft.storage/storageaccounts
@description('Default storage account.')
resource Storage_storageAccounts_Default 'Microsoft.Storage/storageAccounts@2023-05-01' = {
	kind: 'StorageV2'
	location: location
	name: baseName
	properties: {
		accessTier: 'Hot'
		allowBlobPublicAccess: false
		allowSharedKeyAccess: false
		defaultToOAuthAuthentication: true
		minimumTlsVersion: 'TLS1_2'
		supportsHttpsTrafficOnly: true
	}
	sku: {
		name: 'Standard_LRS'
	}
	tags: union(
		tags,
		{
			'hidden-title': 'default'
		}
	)
	// https://learn.microsoft.com/azure/templates/microsoft.storage/storageaccounts/queueservices
	resource queueServices_Default 'queueServices' = {
		name: 'default'
		// https://learn.microsoft.com/azure/templates/microsoft.storage/storageaccounts/queueservices/queues
		resource queues_Commands 'queues' = {
			name: 'commands'
		}
	}
}

resource Storage_storageAccounts_Default_queueServices_Default_queues_Commands_authorization 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
	name: guid(
		Storage_storageAccounts_Default::queueServices_Default::queues_Commands.id,
		deployer().objectId,
		'Storage Queue Data Contributor'
	)
	properties: {
		roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '974c5e8b-45b9-4653-ba55-5f855dd0fb88')
		principalId: deployer().objectId
	}
	scope: Storage_storageAccounts_Default::queueServices_Default::queues_Commands
}

/* outputs */

output insightsAuthOffIngestionEndpoint string = getIngestionEndpoint(Insights_components_AuthOff.properties.ConnectionString)
output insightsAuthOffInstrumentationKey string = Insights_components_AuthOff.properties.InstrumentationKey
output insightsAuthOnIngestionEndpoint string = getIngestionEndpoint(Insights_components_AuthOn.properties.ConnectionString)
output insightsAuthOnInstrumentationKey string = Insights_components_AuthOn.properties.InstrumentationKey
output storageDefaultQueueEndpoint string = Storage_storageAccounts_Default.properties.primaryEndpoints.queue

/* functions */

@description('Get IngestionEndpoint from ConnectionString.')
func getIngestionEndpoint(connectionString string) string => split(filter(split(connectionString, ';'), item => startsWith(item, 'IngestionEndpoint'))[0], '=')[1]
