param name string
param location string = resourceGroup().location
param containerAppEnvironmentId string = '"/subscriptions/c1fdca6a-af12-4a5a-90d3-fd64c554d9e7/resourceGroups/apwaug3/providers/Microsoft.App/connectedEnvironments/apwaug3-kubeenv'
param repositoryImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
param envVars array = []
param registry string
param minReplicas int = 1
param maxReplicas int = 1
param port int = 80
param externalIngress bool = false
param allowInsecure bool = true
param transport string = 'http'
param appProtocol string = 'http'
param registryUsername string
@secure()
param registryPassword string

resource containerApp 'Microsoft.App/containerApps@2022-01-01-preview' ={
  name: name
  location: location
  extendedLocation: {
        name: '/subscriptions/c1fdca6a-af12-4a5a-90d3-fd64c554d9e7/resourceGroups/apwaug3/providers/Microsoft.ExtendedLocation/customLocations/apwaug3',
        type: 'CustomLocation'
    },
  properties:{
    environmentId: containerAppEnvironmentId
    configuration: {
      dapr: {
        enabled: true
        appId: name
        appPort: port
        appProtocol: appProtocol
      }
      activeRevisionsMode: 'single'
      secrets: [
        {
          name: 'container-registry-password'
          value: registryPassword
        }
      ]      
      registries: [
        {
          server: registry
          username: registryUsername
          passwordSecretRef: 'container-registry-password'
        }
      ]
      ingress: {
        external: externalIngress
        targetPort: port
        transport: transport
        allowInsecure: allowInsecure
      }
    }
    template: {
      containers: [
        {
          image: repositoryImage
          name: name
          env: envVars
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
