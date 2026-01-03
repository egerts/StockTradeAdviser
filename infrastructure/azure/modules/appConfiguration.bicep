@description('The name of the App Configuration store')
param name string

@description('The location where the App Configuration store will be deployed')
param location string

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2020-07-01-preview' = {
  name: name
  location: location
  sku: {
    name: 'free'
  }
}

output endpoint string = appConfiguration.properties.endpoint
