#!/bin/bash
# Quick deployment script for Indiana ChatBot infrastructure
# This script deploys the Bing Custom Search resource to Azure

set -e

# Determine script directory and repository root
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Check if we're in the infrastructure directory or repository root
if [[ -f "$SCRIPT_DIR/main.bicep" ]]; then
    # Running from infrastructure directory
    TEMPLATE_FILE="$SCRIPT_DIR/main.bicep"
    PARAMETERS_FILE="$SCRIPT_DIR/main.parameters.json"
elif [[ -f "$SCRIPT_DIR/../infrastructure/main.bicep" ]]; then
    # Running from repository root
    TEMPLATE_FILE="$SCRIPT_DIR/../infrastructure/main.bicep"
    PARAMETERS_FILE="$SCRIPT_DIR/../infrastructure/main.parameters.json"
else
    echo "❌ Cannot find Bicep templates. Please run this script from the repository root or infrastructure directory."
    exit 1
fi

echo "================================================"
echo "Indiana ChatBot - Infrastructure Deployment"
echo "================================================"
echo ""

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo "❌ Azure CLI is not installed."
    echo "Please install it from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

echo "✅ Azure CLI found"
echo ""

# Check if user is logged in
if ! az account show &> /dev/null; then
    echo "🔐 Not logged in to Azure. Logging in..."
    az login
else
    echo "✅ Already logged in to Azure"
fi

echo ""

# Get current subscription
SUBSCRIPTION=$(az account show --query name -o tsv)
echo "📋 Current subscription: $SUBSCRIPTION"
echo ""

read -p "Do you want to use this subscription? (y/n) " -n 1 -r
echo ""

if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Please set your desired subscription with: az account set --subscription <name-or-id>"
    exit 1
fi

echo ""
echo "🚀 Starting deployment..."
echo ""

# Deploy the infrastructure
DEPLOYMENT_NAME="indianachatbot-$(date +%Y%m%d-%H%M%S)"

az deployment sub create \
  --name "$DEPLOYMENT_NAME" \
  --location eastus \
  --template-file "$TEMPLATE_FILE" \
  --parameters "$PARAMETERS_FILE"

echo ""
echo "================================================"
echo "✅ Deployment completed successfully!"
echo "================================================"
echo ""

# Get outputs
echo "📊 Deployment Outputs:"
echo ""

RESOURCE_GROUP=$(az deployment sub show --name "$DEPLOYMENT_NAME" --query properties.outputs.resourceGroupName.value -o tsv)
BING_SEARCH_NAME=$(az deployment sub show --name "$DEPLOYMENT_NAME" --query properties.outputs.bingSearchAccountName.value -o tsv)
ENDPOINT=$(az deployment sub show --name "$DEPLOYMENT_NAME" --query properties.outputs.bingSearchEndpoint.value -o tsv)

echo "Resource Group: $RESOURCE_GROUP"
echo "Bing Search Account: $BING_SEARCH_NAME"
echo "Endpoint: $ENDPOINT"
echo ""

# Get API keys
echo "🔑 Retrieving API keys..."
echo ""

API_KEY=$(az cognitiveservices account keys list \
  --name "$BING_SEARCH_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query key1 -o tsv)

echo "================================================"
echo "📝 Configuration for appsettings.json"
echo "================================================"
echo ""
echo "Add the following to IndianaChatBot/appsettings.json:"
echo ""
echo '  "BingSearch": {'
echo '    "ApiKey": "'"$API_KEY"'",'
echo '    "CustomConfigId": "YOUR_CUSTOM_CONFIG_ID"'
echo '  }'
echo ""
echo "Note: Create a Custom Search instance at https://www.customsearch.ai/"
echo "      to get your CustomConfigId"
echo ""
echo "================================================"
echo "✅ Setup complete!"
echo "================================================"
