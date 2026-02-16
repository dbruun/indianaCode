# Infrastructure Deployment

This directory contains Azure Bicep templates for deploying the Indiana ChatBot infrastructure, specifically the Bing Custom Search resource.

## Overview

The Bicep templates automatically deploy:
- **Resource Group**: A dedicated resource group for the Indiana ChatBot resources
- **Bing Custom Search**: A Cognitive Services Bing Search v7 account for grounding AI responses

## Prerequisites

- Azure CLI installed ([Install Guide](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli))
- An active Azure subscription
- Appropriate permissions to create resource groups and Cognitive Services resources

## Quick Start

### 1. Login to Azure

```bash
az login
```

### 2. Set Your Subscription (if you have multiple)

```bash
az account set --subscription "Your-Subscription-Name-or-ID"
```

### 3. Deploy the Infrastructure

```bash
# Deploy with default parameters
az deployment sub create \
  --location eastus \
  --template-file infrastructure/main.bicep

# Or deploy with custom parameters
az deployment sub create \
  --location eastus \
  --template-file infrastructure/main.bicep \
  --parameters infrastructure/main.parameters.json
```

### 4. Retrieve API Keys

After deployment completes, get the API key:

```bash
# Get the Bing Search account name from outputs
BING_SEARCH_NAME=$(az deployment sub show \
  --name main \
  --query properties.outputs.bingSearchAccountName.value \
  -o tsv)

RESOURCE_GROUP=$(az deployment sub show \
  --name main \
  --query properties.outputs.resourceGroupName.value \
  -o tsv)

# List the API keys
az cognitiveservices account keys list \
  --name $BING_SEARCH_NAME \
  --resource-group $RESOURCE_GROUP
```

### 5. Configure Your Application

Copy the API key and update your `IndianaChatBot/appsettings.json`:

```json
{
  "BingSearch": {
    "ApiKey": "YOUR_API_KEY_FROM_STEP_4",
    "CustomConfigId": "YOUR_CUSTOM_CONFIG_ID"
  }
}
```

**Note**: You'll need to create a Custom Search instance at [Bing Custom Search Portal](https://www.customsearch.ai/) to get the `CustomConfigId`.

## Files

- **main.bicep**: Main template that creates the resource group and orchestrates deployment
- **bingSearch.bicep**: Module that creates the Bing Custom Search resource
- **main.parameters.json**: Parameter file for customizing the deployment

## Customization

### Change Resource Names or Location

Edit `main.parameters.json`:

```json
{
  "parameters": {
    "resourceGroupName": {
      "value": "my-custom-rg-name"
    },
    "location": {
      "value": "westus2"
    },
    "bingSearchSku": {
      "value": "S1"
    }
  }
}
```

### Available SKUs

- **F1**: Free tier (limited requests per month)
- **S1**: Standard tier (pay-as-you-go)

### Available Regions

Bing Search v7 is available in most Azure regions. Common options:
- `eastus`
- `westus2`
- `northeurope`
- `westeurope`
- `southeastasia`

Check availability with:
```bash
az provider show --namespace Microsoft.CognitiveServices \
  --query "resourceTypes[?resourceType=='accounts'].locations" -o table
```

## Deployment Commands

### Deploy to a specific subscription

```bash
az deployment sub create \
  --location eastus \
  --template-file infrastructure/main.bicep \
  --subscription "subscription-id"
```

### What-If Deployment (preview changes)

```bash
az deployment sub what-if \
  --location eastus \
  --template-file infrastructure/main.bicep \
  --parameters infrastructure/main.parameters.json
```

### Validate Template

```bash
az deployment sub validate \
  --location eastus \
  --template-file infrastructure/main.bicep \
  --parameters infrastructure/main.parameters.json
```

## Cleanup

To remove all deployed resources:

```bash
# Get the resource group name
RESOURCE_GROUP=$(az deployment sub show \
  --name main \
  --query properties.outputs.resourceGroupName.value \
  -o tsv)

# Delete the resource group and all resources
az group delete --name $RESOURCE_GROUP --yes
```

## Creating a Custom Search Instance

After deploying the Bing Search resource, you need to create a Custom Search configuration:

1. Go to [Bing Custom Search Portal](https://www.customsearch.ai/)
2. Sign in with your Microsoft account
3. Create a new custom search instance
4. Configure your search settings (websites to search, etc.)
5. In the Production tab, note your **Custom Configuration ID**
6. Use this ID in your application's `appsettings.json`

## Troubleshooting

### Error: "Location not supported"

Try a different region. Use the command above to check available locations.

### Error: "Quota exceeded"

You may have reached your subscription limit for Cognitive Services. Check your quota or use a different subscription.

### Can't find API keys

Make sure the deployment completed successfully:

```bash
az deployment sub show --name main --query properties.provisioningState
```

## Security Best Practices

1. **Never commit API keys** to source control
2. **Use Key Vault** in production:
   ```bash
   # Store key in Key Vault
   az keyvault secret set \
     --vault-name myKeyVault \
     --name BingSearchApiKey \
     --value "your-api-key"
   ```
3. **Use Managed Identity** when running in Azure (App Service, Container Apps, etc.)
4. **Implement rate limiting** to control costs
5. **Monitor usage** through Azure Portal

## Cost Considerations

- **F1 (Free)**: Limited to 1,000 transactions per month
- **S1 (Standard)**: Pay-per-use pricing

Check current pricing: [Bing Search API Pricing](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/search-api/)

## Support

For issues with:
- **Bicep templates**: Check the [Bicep documentation](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
- **Bing Search API**: Check the [API documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/bing-web-search/)
- **Application setup**: See the main [README.md](../README.md)

## Additional Resources

- [Azure Bicep Documentation](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
- [Bing Custom Search Documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/bing-custom-search/)
- [Azure CLI Documentation](https://docs.microsoft.com/en-us/cli/azure/)
