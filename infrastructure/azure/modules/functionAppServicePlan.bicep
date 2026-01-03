@description('The name of the Function App Service Plan')
param name string

@description('The location where the Function App Service Plan will be deployed')
param location string

resource functionAppServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: name
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: false
    perSiteScaling: false
    elasticScaleEnabled: false
    maximumElasticWorkerCount: 1
  }
  kind: 'functionapp'
}

output id string = functionAppServicePlan.id
