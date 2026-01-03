@description('The name of the App Service Plan')
param name string

@description('The location where the App Service Plan will be deployed')
param location string

resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: name
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
  }
  properties: {
    reserved: false
  }
}

output id string = appServicePlan.id
