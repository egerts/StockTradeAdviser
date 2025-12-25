targetScope = 'resourceGroup'

param location string = resourceGroup().location
param sqlAdministratorLogin string
param sqlAdministratorPassword string
param functionAppName string = 'StockTradeAdviser-Functions'
param webAppName string = 'StockTradeAdviser-API'
param storageAccountName string = 'stocktradeadviser${uniqueString(resourceGroup().id)}'
param cosmosDbAccountName string = 'stocktradeadviser-cosmos${uniqueString(resourceGroup().id)}'
param serviceBusNamespaceName string = 'stocktradeadviser-sb${uniqueString(resourceGroup().id)}'
param appInsightsName string = 'stocktradeadviser-insights'

var storageAccountId = storageAccount.id
var cosmosDbEndpoint = cosmosDbAccount.properties.documentEndpoint
var serviceBusEndpoint = serviceBusNamespace.properties.serviceBusEndpoint

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmosDbAccountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    databaseAccountOfferType: 'Standard'
  }
}

resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  parent: cosmosDbAccount
  name: 'StockTradeAdviser'
  properties: {
    resource: {
      id: 'StockTradeAdviser'
    }
  }
}

resource usersContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: cosmosDatabase
  name: 'users'
  properties: {
    resource: {
      id: 'users'
      partitionKey: {
        paths: ['/id']
        kind: 'Hash'
        version: 2
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
    }
  }
}

resource portfoliosContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: cosmosDatabase
  name: 'portfolios'
  properties: {
    resource: {
      id: 'portfolios'
      partitionKey: {
        paths: ['/userId']
        kind: 'Hash'
        version: 2
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
    }
  }
}

resource stocksContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: cosmosDatabase
  name: 'stocks'
  properties: {
    resource: {
      id: 'stocks'
      partitionKey: {
        paths: ['/symbol']
        kind: 'Hash'
        version: 2
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
    }
  }
}

resource recommendationsContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: cosmosDatabase
  name: 'recommendations'
  properties: {
    resource: {
      id: 'recommendations'
      partitionKey: {
        paths: ['/userId']
        kind: 'Hash'
        version: 2
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
    }
  }
}

resource recommendationHistoryContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: cosmosDatabase
  name: 'recommendationHistory'
  properties: {
    resource: {
      id: 'recommendationHistory'
      partitionKey: {
        paths: ['/userId']
        kind: 'Hash'
        version: 2
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
    }
  }
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
  }
}

resource stockDataQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'stock-data-queue'
  properties: {
    lockDuration: 'PT1M'
    defaultMessageTimeToLive: 'P14D'
    deadLetteringOnMessageExpiration: true
    enablePartitioning: true
    enableExpress: true
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'stocktradeadviser-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource functionAppStorage 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: '${functionAppName}storage${uniqueString(resourceGroup().id)}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: functionAppPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${functionAppStorage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(functionAppStorage.id, functionAppStorage.apiVersion).keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${functionAppStorage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(functionAppStorage.id, functionAppStorage.apiVersion).keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: functionAppName
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
          name: 'CosmosDb:Endpoint'
          value: cosmosDbEndpoint
        }
        {
          name: 'CosmosDb:Key'
          value: cosmosDbAccount.listKeys().primaryMasterKey
        }
        {
          name: 'CosmosDb:DatabaseName'
          value: 'StockTradeAdviser'
        }
        {
          name: 'ServiceBus:ConnectionString'
          value: 'Endpoint=${serviceBusEndpoint};SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=${listKeys(serviceBusNamespace.id, serviceBusNamespace.apiVersion).primaryKey}'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'AlphaVantage:ApiKey'
          value: ''
        }
      ]
      cors: {
        allowedOrigins: [
          '*'
        ]
        supportCredentials: false
      }
    }
  }
}

resource functionAppPlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: '${functionAppName}-plan'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: false
  }
}

resource webAppPlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: '${webAppName}-plan'
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  properties: {
    reserved: false
  }
}

resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: webAppName
  location: location
  kind: 'app'
  properties: {
    serverFarmId: webAppPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'CosmosDb:Endpoint'
          value: cosmosDbEndpoint
        }
        {
          name: 'CosmosDb:Key'
          value: cosmosDbAccount.listKeys().primaryMasterKey
        }
        {
          name: 'CosmosDb:DatabaseName'
          value: 'StockTradeAdviser'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'AzureAd:Instance'
          value: 'https://login.microsoftonline.com/'
        }
        {
          name: 'AzureAd:Domain'
          value: ''
        }
        {
          name: 'AzureAd:TenantId'
          value: ''
        }
        {
          name: 'AzureAd:ClientId'
          value: ''
        }
        {
          name: 'AzureAd:ClientSecret'
          value: ''
        }
      ]
      cors: {
        allowedOrigins: [
          'http://localhost:3000'
          'https://localhost:3000'
        ]
        supportCredentials: true
      }
    }
  }
}

resource staticWebsite 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    staticWebsite: {
      enabled: true
      indexDocument: 'index.html'
      errorDocument404Path: 'index.html'
    }
  }
}

output storageAccountName string = storageAccount.name
output cosmosDbEndpoint string = cosmosDbEndpoint
output cosmosDbKey string = cosmosDbAccount.listKeys().primaryMasterKey
output serviceBusEndpoint string = serviceBusEndpoint
output serviceBusPrimaryKey string = listKeys(serviceBusNamespace.id, serviceBusNamespace.apiVersion).primaryKey
output functionAppName string = functionApp.name
output webAppName string = webApp.name
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
output storageAccountConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
