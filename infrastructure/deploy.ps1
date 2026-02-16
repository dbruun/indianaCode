# Quick deployment script for Indiana ChatBot infrastructure
# This script deploys the Bing Custom Search resource to Azure

$ErrorActionPreference = "Stop"

# Determine script directory and find templates
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Check if we're in the infrastructure directory or repository root
if (Test-Path (Join-Path $ScriptDir "main.bicep")) {
    # Running from infrastructure directory
    $TemplateFile = Join-Path $ScriptDir "main.bicep"
    $ParametersFile = Join-Path $ScriptDir "main.parameters.json"
} elseif (Test-Path (Join-Path $ScriptDir "..\infrastructure\main.bicep")) {
    # Running from repository root
    $TemplateFile = Join-Path $ScriptDir "..\infrastructure\main.bicep"
    $ParametersFile = Join-Path $ScriptDir "..\infrastructure\main.parameters.json"
} else {
    Write-Host "❌ Cannot find Bicep templates. Please run this script from the repository root or infrastructure directory." -ForegroundColor Red
    exit 1
}

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Indiana ChatBot - Infrastructure Deployment" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if Azure CLI is installed
try {
    $null = Get-Command az -ErrorAction Stop
    Write-Host "✅ Azure CLI found" -ForegroundColor Green
} catch {
    Write-Host "❌ Azure CLI is not installed." -ForegroundColor Red
    Write-Host "Please install it from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
}

Write-Host ""

# Check if user is logged in
try {
    $null = az account show 2>$null
    Write-Host "✅ Already logged in to Azure" -ForegroundColor Green
} catch {
    Write-Host "🔐 Not logged in to Azure. Logging in..." -ForegroundColor Yellow
    az login
}

Write-Host ""

# Get current subscription
$subscription = az account show --query name -o tsv
Write-Host "📋 Current subscription: $subscription" -ForegroundColor Cyan
Write-Host ""

$response = Read-Host "Do you want to use this subscription? (y/n)"
if ($response -notmatch '^[Yy]$') {
    Write-Host "Please set your desired subscription with: az account set --subscription <name-or-id>" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "🚀 Starting deployment..." -ForegroundColor Yellow
Write-Host ""

# Deploy the infrastructure
$deploymentName = "indianachatbot-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

az deployment sub create `
  --name $deploymentName `
  --location eastus `
  --template-file $TemplateFile `
  --parameters $ParametersFile

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Deployment failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "✅ Deployment completed successfully!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

# Get outputs
Write-Host "📊 Deployment Outputs:" -ForegroundColor Cyan
Write-Host ""

$resourceGroup = az deployment sub show --name $deploymentName --query properties.outputs.resourceGroupName.value -o tsv
$bingSearchName = az deployment sub show --name $deploymentName --query properties.outputs.bingSearchAccountName.value -o tsv
$endpoint = az deployment sub show --name $deploymentName --query properties.outputs.bingSearchEndpoint.value -o tsv

Write-Host "Resource Group: $resourceGroup"
Write-Host "Bing Search Account: $bingSearchName"
Write-Host "Endpoint: $endpoint"
Write-Host ""

# Get API keys
Write-Host "🔑 Retrieving API keys..." -ForegroundColor Yellow
Write-Host ""

$apiKey = az cognitiveservices account keys list `
  --name $bingSearchName `
  --resource-group $resourceGroup `
  --query key1 -o tsv

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "📝 Configuration for appsettings.json" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Add the following to IndianaChatBot/appsettings.json:"
Write-Host ""
Write-Host '  "BingSearch": {'
Write-Host '    "ApiKey": "' -NoNewline
Write-Host $apiKey -NoNewline -ForegroundColor Yellow
Write-Host '",'
Write-Host '    "CustomConfigId": "YOUR_CUSTOM_CONFIG_ID"'
Write-Host '  }'
Write-Host ""
Write-Host "Note: Create a Custom Search instance at https://www.customsearch.ai/" -ForegroundColor Yellow
Write-Host "      to get your CustomConfigId" -ForegroundColor Yellow
Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "✅ Setup complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
