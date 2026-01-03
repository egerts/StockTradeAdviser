@description('The name of the Storage Account')
param name string

@description('The location where the Storage Account will be deployed')
param location string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' = {
  name: name
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
    accessTier: 'Hot'
  }
}

output id string = storageAccount.id
output primaryConnectionString string: string = listKeys(storageAccount.id, '2021-09-01').connectionStrings[0].connectionString
