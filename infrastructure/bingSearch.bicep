// Bicep module for Bing Custom Search resource
// This module creates a Cognitive Services Bing Search v7 account

@description('Name of the Bing Custom Search account')
param bingSearchName string

@description('Location for the resource')
param location string

@description('SKU for Bing Custom Search')
@allowed([
  'F1' // Free tier
  'S1' // Standard tier
])
param sku string = 'S1'

@description('Tags to apply to the resource')
param tags object = {}

// Bing Custom Search is part of Cognitive Services
resource bingSearchAccount 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: bingSearchName
  location: location
  tags: tags
  kind: 'Bing.Search.v7'
  sku: {
    name: sku
  }
  properties: {
    apiProperties: {
      statisticsEnabled: false
    }
    customSubDomainName: bingSearchName
    publicNetworkAccess: 'Enabled'
  }
}

// Outputs
@description('Bing Custom Search resource name')
output bingSearchName string = bingSearchAccount.name

@description('Bing Custom Search resource ID')
output bingSearchId string = bingSearchAccount.id

@description('Bing Custom Search endpoint')
output endpoint string = bingSearchAccount.properties.endpoint
