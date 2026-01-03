@description('The name of the Function App')
param name string

@description('The location where the Function App will be deployed')
param location string

@description('The ID of the Function App Service Plan')
param functionAppServicePlanId string

@description('The ID of the Application Insights component')
param applicationInsightsId string

@description('The connection string for Azure Storage')
param storageConnectionString string

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: name
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: functionAppServicePlanId
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storageConnectionString
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: reference(applicationInsightsId, '2015-05-01').InstrumentationKey
        }
      ]
      alwaysOn: false
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      remoteDebuggingEnabled: false
    }
    httpsOnly: true
    clientAffinityEnabled: false
  }
}

resource functionAppConfig 'Microsoft.Web/sites/config@2021-03-01' = {
  parent: functionApp
  name: 'web'
  properties: {
    numberOfWorkers: 1
    functionAppScaleLimit: 200
    functionsRuntimeScaleMonitoringEnabled: false
  }
}

output id string = functionApp.id
output defaultHostName string = functionApp.properties.defaultHostName
