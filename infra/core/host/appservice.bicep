param name string
param location string = resourceGroup().location
param tags object = {}

param appServicePlanId string
param managedIdentity bool = false

param runtimeName string 
param runtimeVersion string

param appSettings object = {}

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: name
  location: location
  tags: tags
  identity: managedIdentity ? { type: 'SystemAssigned' } : { type: 'None' }
  properties: {
    serverFarmId: appServicePlanId
    siteConfig: {
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: runtimeName
        }
      ]
      netFrameworkVersion: runtimeName == 'dotnet' ? 'v8.0' : null
      appSettings: [for key in items(appSettings): {
        name: key.key
        value: key.value
      }]
    }
    httpsOnly: true
  }
}

output id string = appService.id
output name string = appService.name
output uri string = 'https://${appService.properties.defaultHostName}'
output identityPrincipalId string = managedIdentity ? appService.identity.principalId : ''