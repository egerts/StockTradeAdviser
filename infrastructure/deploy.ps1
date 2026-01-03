# Azure Deployment Script for StockTradeAdviser (Bicep)
# This script creates all necessary Azure resources using Bicep modules

param(
    [string]$AppName = "stocktradeadviser",
    [string]$Location = "swedencentral"
)

$ErrorActionPreference = "Stop"

$ResourceGroup = "$AppName-rg"

Write-Host "========================================" -ForegroundColor Red
Write-Host "WARNING: This will deploy to Azure!" -ForegroundColor Red
Write-Host "Please verify your Azure account before proceeding." -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""
Write-Host "Target Configuration:" -ForegroundColor Yellow
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor White
Write-Host "Location: $Location" -ForegroundColor White
Write-Host "App Name: $AppName" -ForegroundColor White
Write-Host ""

# Check if Azure CLI is installed
try {
    az --version | Out-Null
    Write-Host "Azure CLI found" -ForegroundColor Green
} catch {
    Write-Host "Azure CLI is not installed. Please install it first:" -ForegroundColor Red
    Write-Host "https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Red
    exit 1
}

# Force logout and login every time for safety
Write-Host "Forcing fresh Azure login..." -ForegroundColor Blue
az logout | Out-Null
Write-Host "Please login to your Azure account:" -ForegroundColor Yellow
az login

# Show current account details for verification
Write-Host ""
Write-Host "Current Azure Account:" -ForegroundColor Cyan
$CurrentAccount = az account show | ConvertFrom-Json
Write-Host "Subscription: $($CurrentAccount.name)" -ForegroundColor White
Write-Host "Tenant: $($CurrentAccount.tenantDisplayName)" -ForegroundColor White
Write-Host "User: $($CurrentAccount.user.name)" -ForegroundColor White
Write-Host ""

# Confirm deployment with user
Write-Host "DEPLOYMENT CONFIRMATION" -ForegroundColor Red
Write-Host "You are about to deploy resources to the account listed above." -ForegroundColor Red
Write-Host "This will create resources that may incur costs." -ForegroundColor Red
Write-Host ""
$Confirmation = Read-Host "Type 'DEPLOY' to continue, or anything else to cancel"
if ($Confirmation -ne "DEPLOY") {
    Write-Host "Deployment cancelled by user." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Starting Azure deployment for StockTradeAdviser..." -ForegroundColor Green

# Create Resource Group
Write-Host "Creating resource group..." -ForegroundColor Blue
az group create `
    --name "$ResourceGroup" `
    --location "$Location" `
    --tags "project=stocktradeadviser" "environment=production"

# Deploy infrastructure using Bicep
Write-Host "Deploying infrastructure with Bicep..." -ForegroundColor Blue
$DeploymentOutput = az deployment group create `
    --resource-group "$ResourceGroup" `
    --template-file "./azure/main.bicep" `
    --parameters "./azure/parameters.json" `
    --parameters "appName=$AppName" `
    --output json | ConvertFrom-Json

# Extract outputs
$ApiUrl = $DeploymentOutput.properties.outputs.apiUrl.value
$FunctionAppUrl = $DeploymentOutput.properties.outputs.functionAppUrl.value
$CosmosEndpoint = $DeploymentOutput.properties.outputs.cosmosEndpoint.value
$AppConfigEndpoint = $DeploymentOutput.properties.outputs.appConfigurationEndpoint.value
$ServiceBusConnectionString = $DeploymentOutput.properties.outputs.serviceBusConnectionString.value

Write-Host ""
Write-Host "Infrastructure deployed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Deployment Summary:" -ForegroundColor Yellow
Write-Host "API URL: $ApiUrl" -ForegroundColor Cyan
Write-Host "Function App URL: $FunctionAppUrl" -ForegroundColor Cyan
Write-Host "Cosmos DB Endpoint: $CosmosEndpoint" -ForegroundColor Cyan
Write-Host "App Configuration Endpoint: $AppConfigEndpoint" -ForegroundColor Cyan
Write-Host "Service Bus Connection: [CONFIGURED]" -ForegroundColor Cyan
Write-Host ""

# Get Cosmos DB connection key
Write-Host "Getting Cosmos DB connection key..." -ForegroundColor Blue
$CosmosKey = az cosmosdb keys list `
    --resource-group "$ResourceGroup" `
    --name "$AppName-cosmos" `
    --type keys `
    --query "primaryMasterKey" `
    --output tsv

Write-Host "Cosmos DB Key: [REDACTED]" -ForegroundColor Yellow
Write-Host ""

# Save configuration to .env file
Write-Host "Creating configuration files..." -ForegroundColor Blue

# Backend .env.production
$BackendEnv = @"
# Azure Production Configuration
AZURE_COSMOS_ENDPOINT=$CosmosEndpoint
AZURE_COSMOS_KEY=$CosmosKey
AZURE_APP_CONFIGURATION_ENDPOINT=$AppConfigEndpoint
API_BASE_URL=$ApiUrl
SERVICEBUS_CONNECTION_STRING=$ServiceBusConnectionString
"@

$BackendEnv | Out-File -FilePath "../.env.production" -Encoding UTF8
Write-Host "Created: ../.env.production" -ForegroundColor Green

# Frontend .env.production
$FrontendEnv = @"
# Frontend Production Configuration
REACT_APP_API_BASE_URL=$ApiUrl
REACT_APP_AZURE_AD_CLIENT_ID=your-client-id-here
REACT_APP_AZURE_AD_TENANT_ID=your-tenant-id-here
"@

$FrontendEnv | Out-File -FilePath "../frontend/.env.production" -Encoding UTF8
Write-Host "Created: ../frontend/.env.production" -ForegroundColor Green

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Update the backend to use Azure services (replace mock services)" -ForegroundColor White
Write-Host "2. Configure Azure AD B2C for authentication" -ForegroundColor White
Write-Host "3. Update frontend environment variables with Azure AD details" -ForegroundColor White
Write-Host "4. Deploy applications to Azure" -ForegroundColor White
Write-Host "5. Deploy Function App to: $FunctionAppUrl" -ForegroundColor White
Write-Host ""
Write-Host "Important Notes:" -ForegroundColor Red
Write-Host "- Save the Cosmos DB key and App Configuration connection string securely" -ForegroundColor White
Write-Host "- The API URL is: $ApiUrl" -ForegroundColor Cyan
Write-Host "- The Function App URL is: $FunctionAppUrl" -ForegroundColor Cyan
Write-Host "- Service Bus queue 'stock-data-queue' is ready for use" -ForegroundColor Green
Write-Host "- Function App uses Y1 (Consumption) plan - pay-per-use pricing" -ForegroundColor Green
Write-Host ""

Write-Host "Azure infrastructure setup complete!" -ForegroundColor Green
