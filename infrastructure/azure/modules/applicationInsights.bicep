@description('The name of the Application Insights component')
param name string

@description('The location where the Application Insights component will be deployed')
param location string

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}
