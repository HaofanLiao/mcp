targetScope = 'resourceGroup'

@description('The name of the Web PubSub service')
param baseName string = resourceGroup().name

@description('The location for all resources')
param location string = resourceGroup().location

// Web PubSub Service
resource webpubsub 'Microsoft.SignalRService/webPubSub@2024-03-01' = {
  name: baseName
  location: location
  sku: {
    name: 'Standard_S1'
    tier: 'Standard'
    capacity: 1
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

// Basic outputs for tests
output baseName string = webpubsub.name
output webPubSubId string = webpubsub.id

