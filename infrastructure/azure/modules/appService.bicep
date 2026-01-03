@description('The name of the App Service')
param name string

@description('The location where the App Service will be deployed')
param location string

@description('The ID of the App Service Plan')
param appServicePlanId string

resource appService 'Microsoft.Web/sites@2021-03-01' = {
  name: name
  location: location
  properties: {
    serverFarmId: appServicePlanId
    siteConfig: {
      alwaysOn: false
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      remoteDebuggingEnabled: false
    }
    httpsOnly: true
  }
}

output url string = 'https://${name}.azurewebsites.net'
