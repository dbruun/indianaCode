// Main Bicep template for deploying Bing Custom Search resource
// This template creates a resource group and a Bing Custom Search resource

targetScope = 'subscription'

@description('Name of the resource group to create')
param resourceGroupName string = 'rg-indianachatbot'

@description('Location for all resources')
param location string = 'eastus'

@description('Name of the Bing Custom Search account')
param bingSearchName string = 'bing-search-${uniqueString(subscription().subscriptionId, resourceGroupName)}'

@description('SKU for Bing Custom Search (S1 is the standard tier)')
@allowed([
  'F1' // Free tier
  'S1' // Standard tier
])
param bingSearchSku string = 'S1'

@description('Tags to apply to resources')
param tags object = {
  Environment: 'Production'
  Application: 'IndianaChatBot'
  ManagedBy: 'Bicep'
}

// Create the resource group
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Deploy Bing Custom Search resource to the resource group
module bingSearch 'bingSearch.bicep' = {
  name: 'bingSearchDeployment'
  scope: resourceGroup
  params: {
    bingSearchName: bingSearchName
    location: location
    sku: bingSearchSku
    tags: tags
  }
}

// Outputs
@description('Resource group name')
output resourceGroupName string = resourceGroup.name

@description('Bing Custom Search account name')
output bingSearchAccountName string = bingSearch.outputs.bingSearchName

@description('Bing Custom Search endpoint')
output bingSearchEndpoint string = bingSearch.outputs.endpoint

@description('Instructions for getting API key')
output getApiKeyInstructions string = 'Run: az cognitiveservices account keys list --name ${bingSearch.outputs.bingSearchName} --resource-group ${resourceGroup.name}'
