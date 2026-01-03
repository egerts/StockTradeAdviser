# Azure Infrastructure - StockTradeAdviser

This directory contains the Azure infrastructure as code using Bicep with a modular architecture.

## 📁 Directory Structure

```
infrastructure/
├── azure/
│   ├── main.bicep          # Main Bicep template
│   ├── parameters.json     # Deployment parameters
│   └── modules/            # Reusable Bicep modules
│       ├── appServicePlan.bicep
│       ├── appService.bicep
│       ├── cosmosDb.bicep
│       ├── appConfiguration.bicep
│       └── applicationInsights.bicep
└── deploy.ps1             # Deployment script
```

## 🚀 Quick Start

### Prerequisites
1. **Azure CLI** - Install from [Microsoft docs](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
2. **Bicep CLI** - Install with `az bicep install`

### Deployment
```powershell
# From project root
.\setup-azure.ps1

# Or directly
cd infrastructure
.\deploy.ps1
```

## 🏗️ Architecture

### Modular Design
Each Azure resource is defined in its own module for:
- **Reusability** - Use modules across different environments
- **Maintainability** - Easier to update individual resources
- **Testing** - Test modules independently
- **Readability** - Cleaner, more organized code

### Resources Created
| Module | Resource | Purpose | Tier |
|--------|----------|---------|------|
| `appServicePlan` | App Service Plan | Hosting plan | Free F1 |
| `appService` | App Service | Backend API | Free |
| `cosmosDb` | Cosmos DB | Database | Free 400 RU/s |
| `appConfiguration` | App Configuration | Settings | Free |
| `applicationInsights` | Application Insights | Monitoring | Free |

## 📋 Configuration Files

After deployment, the script creates:
- `../.env.production` - Backend configuration
- `../frontend/.env.production` - Frontend configuration

## 🔧 Customization

### Adding New Resources
1. Create a new module in `modules/`
2. Reference it in `azure/main.bicep`
3. Update deployment script if needed

### Modifying Existing Resources
Edit the specific module file in `modules/` without affecting others.

### Environment-Specific Configurations
Create different parameter files:
- `azure/parameters-dev.json`
- `azure/parameters-prod.json`

## 📊 Benefits of Bicep

- **Simpler Syntax** - More concise than ARM templates
- **Modular Architecture** - Reusable components
- **Type Safety** - Better validation and IntelliSense
- **Dependency Management** - Automatic resource ordering
- **Integration** - Works seamlessly with Azure CLI

## 🛠️ Development Workflow

### Local Development
```bash
# Validate Bicep files
az bicep build azure/main.bicep

# Preview deployment
az deployment group what-if \
  --resource-group stocktradeadviser-rg \
  --template-file azure/main.bicep \
  --parameters azure/parameters.json
```

### CI/CD Integration
The Bicep files can be easily integrated into:
- **GitHub Actions** - `azure/arm-deploy`
- **Azure Pipelines** - `AzureResourceManagerTemplateDeployment`
- **Terraform** - Using `azurerm_resource_group_template_deployment`

## 🔍 Monitoring

After deployment:
1. **Application Insights** - Monitor application performance
2. **Azure Portal** - Resource health and metrics
3. **Azure CLI** - Command-line monitoring

## 💰 Cost Management

All resources use free tier:
- **App Service**: 1 GB storage, 60 CPU minutes/day
- **Cosmos DB**: 400 RU/s, 5 GB storage
- **App Configuration**: 1 GB storage, 1,000 transactions/hr
- **Application Insights**: 5 GB data ingestion/month

## 🆘 Troubleshooting

### Common Issues
1. **Bicep not found** - Run `az bicep install`
2. **Permission errors** - Check Azure subscription permissions
3. **Resource limits** - Verify free tier quota availability

### Useful Commands
```bash
# Check deployment status
az deployment group show \
  --resource-group stocktradeadviser-rg \
  --name main

# List resources
az resource list --resource-group stocktradeadviser-rg

# Delete resource group
az group delete --name stocktradeadviser-rg --yes
```

## 📚 Learn More

- [Bicep Documentation](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
- [Bicep Best Practices](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/best-practices)
- [Azure Free Tier](https://azure.microsoft.com/en-us/free/)
