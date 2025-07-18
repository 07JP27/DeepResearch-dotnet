param name string
param location string = resourceGroup().location
param tags object = {}

param sku object = {
  name: 'F1'
}

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: name
  location: location
  tags: tags
  sku: sku
  properties: {
    reserved: false
  }
}

output id string = appServicePlan.id
output name string = appServicePlan.name