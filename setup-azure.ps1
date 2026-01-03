# Azure Deployment Setup Script

Write-Host "🚀 Setting up Azure deployment for StockTradeAdviser..." -ForegroundColor Green
Write-Host ""
Write-Host "⚠️  SAFETY NOTICE:" -ForegroundColor Red
Write-Host "This script will force a fresh Azure login every time" -ForegroundColor Red
Write-Host "Default location changed to Sweden Central" -ForegroundColor Red
Write-Host "You must type 'DEPLOY' to confirm deployment" -ForegroundColor Red
Write-Host ""

# Check if Azure CLI is installed
try {
    az --version | Out-Null
    Write-Host "✅ Azure CLI found" -ForegroundColor Green
} catch {
    Write-Host "❌ Azure CLI not found. Please install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Red
    exit 1
}

# Check if Bicep CLI is installed
try {
    bicep --version | Out-Null
    Write-Host "✅ Bicep CLI found" -ForegroundColor Green
} catch {
    Write-Host "❌ Bicep CLI not found. Installing..." -ForegroundColor Yellow
    az bicep install
    Write-Host "✅ Bicep CLI installed" -ForegroundColor Green
}

# Navigate to infrastructure directory
Set-Location infrastructure

# Check if deployment files exist
if (-not (Test-Path "azure/main.bicep")) {
    Write-Host "❌ azure/main.bicep not found in infrastructure directory" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path "deploy.ps1")) {
    Write-Host "❌ deploy.ps1 not found in infrastructure directory" -ForegroundColor Red
    exit 1
}

Write-Host "📁 Found Bicep deployment files" -ForegroundColor Green
Write-Host "📁 Infrastructure structure:" -ForegroundColor Yellow
Write-Host "  infrastructure/" -ForegroundColor White
Write-Host "  ├── azure/" -ForegroundColor Cyan
Write-Host "  │   ├── main.bicep" -ForegroundColor Gray
Write-Host "  │   ├── parameters.json" -ForegroundColor Gray
Write-Host "  │   └── modules/" -ForegroundColor Gray
Write-Host "  │       ├── appServicePlan.bicep" -ForegroundColor Gray
Write-Host "  │       ├── appService.bicep" -ForegroundColor Gray
Write-Host "  │       ├── cosmosDb.bicep" -ForegroundColor Gray
Write-Host "  │       ├── appConfiguration.bicep" -ForegroundColor Gray
Write-Host "  │       └── applicationInsights.bicep" -ForegroundColor Gray
Write-Host "  └── deploy.ps1" -ForegroundColor Gray
Write-Host ""
Write-Host "🎯 Ready to deploy to Azure using Bicep!" -ForegroundColor Yellow
Write-Host ""
Write-Host "Deployment will:" -ForegroundColor Yellow
Write-Host "  • Force fresh Azure login" -ForegroundColor White
Write-Host "  • Show account details for verification" -ForegroundColor White
Write-Host "  • Require 'DEPLOY' confirmation" -ForegroundColor White
Write-Host "  • Deploy to Sweden Central region" -ForegroundColor White
Write-Host ""
Write-Host "Resources to be created:" -ForegroundColor Yellow
Write-Host "  • Azure App Service (Free tier)" -ForegroundColor White
Write-Host "  • Azure Cosmos DB (Free tier)" -ForegroundColor White
Write-Host "  • Azure App Configuration (Free tier)" -ForegroundColor White
Write-Host "  • Application Insights (Free tier)" -ForegroundColor White
Write-Host ""
Write-Host "Total cost: $0/month (all free tier resources)" -ForegroundColor Green
Write-Host ""
Write-Host "To start the deployment, run:" -ForegroundColor Cyan
Write-Host "  cd infrastructure" -ForegroundColor White
Write-Host "  .\deploy.ps1" -ForegroundColor White
Write-Host ""
Write-Host "🏗️  Using modular Bicep architecture with safety measures!" -ForegroundColor Blue
