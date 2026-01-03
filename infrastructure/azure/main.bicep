@description('The name of the application')
param appName string = 'stocktradeadviser'

@description('The location where resources will be deployed')
param location string = resourceGroup().location

// App Service Plan
module appServicePlan './modules/appServicePlan.bicep' = {
  name: '${appName}-plan'
  params: {
    name: '${appName}-plan'
    location: location
  }
}

// App Service
module appService './modules/appService.bicep' = {
  name: '${appName}-api'
  params: {
    name: '${appName}-api'
    location: location
    appServicePlanId: appServicePlan.outputs.id
  }
}

// Storage Account for Function App
module storageAccount './modules/storageAccount.bicep' = {
  name: '${appName}-functions-storage'
  params: {
    name: '${appName}storage'
    location: location
  }
}

// Function App Service Plan
module functionAppServicePlan './modules/functionAppServicePlan.bicep' = {
  name: '${appName}-functions-plan'
  params: {
    name: '${appName}-functions-plan'
    location: location
  }
}

// Function App
module functionApp './modules/functionApp.bicep' = {
  name: '${appName}-functions'
  params: {
    name: '${appName}-functions'
    location: location
    functionAppServicePlanId: functionAppServicePlan.outputs.id
    applicationInsightsId: applicationInsights.outputs.id
    storageConnectionString: storageAccount.outputs.primaryConnectionString
  }
}

// Service Bus
module serviceBus './modules/serviceBus.bicep' = {
  name: '${appName}-servicebus'
  params: {
    name: '${appName}-sb'
    location: location
  }
}

// Cosmos DB Account
module cosmosDb './modules/cosmosDb.bicep' = {
  name: '${appName}-cosmos'
  params: {
    name: '${appName}-cosmos'
    location: location
  }
}

// App Configuration
module appConfiguration './modules/appConfiguration.bicep' = {
  name: '${appName}-config'
  params: {
    name: '${appName}-config'
    location: location
  }
}

// Application Insights
module applicationInsights './modules/applicationInsights.bicep' = {
  name: '${appName}-insights'
  params: {
    name: '${appName}-insights'
    location: location
  }
}

// Outputs
output apiUrl string = 'https://${appName}-api.azurewebsites.net'
output functionAppUrl string = 'https://${functionApp.outputs.defaultHostName}'
output cosmosEndpoint string = cosmosDb.outputs.endpoint
output appConfigurationEndpoint string = appConfiguration.outputs.endpoint
output serviceBusConnectionString string = serviceBus.outputs.primaryConnectionString
