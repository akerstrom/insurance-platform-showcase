// Insurance Platform Showcase - Azure Container Apps Infrastructure
// Deploys all services to Azure Container Apps (consumption plan - free tier eligible)

@description('Location for all resources')
param location string = resourceGroup().location

@description('Environment name (dev, staging, prod)')
@allowed(['dev', 'staging', 'prod'])
param environment string = 'dev'

@description('Container registry login server (e.g., myregistry.azurecr.io)')
param containerRegistry string

@description('Container registry username (ACR admin username or service principal)')
@secure()
param containerRegistryUsername string

@description('Container registry password')
@secure()
param containerRegistryPassword string

@description('Container image tag')
param imageTag string = 'latest'

// Resource naming
var prefix = 'ips'
var suffix = environment
var containerAppEnvName = '${prefix}-env-${suffix}'
var logAnalyticsName = '${prefix}-logs-${suffix}'

// Container image names
var images = {
  vehicleService: '${containerRegistry}/ips-vehicle-service:${imageTag}'
  insuranceService: '${containerRegistry}/ips-insurance-service:${imageTag}'
  customerService: '${containerRegistry}/ips-customer-service:${imageTag}'
  legacyVehicleDb: '${containerRegistry}/ips-legacy-vehicle-db:${imageTag}'
  legacyMainframe: '${containerRegistry}/ips-legacy-mainframe:${imageTag}'
}

// Log Analytics Workspace (required for Container Apps)
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Container Apps Environment
resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// Registry configuration for all container apps
var registryConfig = [
  {
    server: containerRegistry
    username: containerRegistryUsername
    passwordSecretRef: 'acr-password'
  }
]

var registrySecrets = [
  {
    name: 'acr-password'
    value: containerRegistryPassword
  }
]

// Legacy Vehicle Database (internal only)
resource legacyVehicleDb 'Microsoft.App/containerApps@2023-05-01' = {
  name: '${prefix}-legacy-vehicle-db'
  location: location
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      secrets: registrySecrets
      registries: registryConfig
      ingress: {
        external: false
        targetPort: 8080
        transport: 'http'
      }
    }
    template: {
      containers: [
        {
          name: 'legacy-vehicle-db'
          image: images.legacyVehicleDb
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

// Legacy Insurance Mainframe (internal only)
resource legacyMainframe 'Microsoft.App/containerApps@2023-05-01' = {
  name: '${prefix}-legacy-mainframe'
  location: location
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      secrets: registrySecrets
      registries: registryConfig
      ingress: {
        external: false
        targetPort: 8080
        transport: 'http'
      }
    }
    template: {
      containers: [
        {
          name: 'legacy-mainframe'
          image: images.legacyMainframe
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

// Vehicle Service (internal only)
resource vehicleService 'Microsoft.App/containerApps@2023-05-01' = {
  name: '${prefix}-vehicle-service'
  location: location
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      secrets: registrySecrets
      registries: registryConfig
      ingress: {
        external: false
        targetPort: 8080
        transport: 'http'
      }
    }
    template: {
      containers: [
        {
          name: 'vehicle-service'
          image: images.vehicleService
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'LegacyServices__VehicleDatabase'
              value: 'http://${legacyVehicleDb.name}'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

// Insurance Service (internal only)
resource insuranceService 'Microsoft.App/containerApps@2023-05-01' = {
  name: '${prefix}-insurance-service'
  location: location
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      secrets: registrySecrets
      registries: registryConfig
      ingress: {
        external: false
        targetPort: 8080
        transport: 'http'
      }
    }
    template: {
      containers: [
        {
          name: 'insurance-service'
          image: images.insuranceService
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'LegacyServices__InsuranceMainframe'
              value: 'http://${legacyMainframe.name}'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

// Customer Service (external - main entry point)
resource customerService 'Microsoft.App/containerApps@2023-05-01' = {
  name: '${prefix}-customer-service'
  location: location
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      secrets: registrySecrets
      registries: registryConfig
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        corsPolicy: {
          allowedOrigins: ['*']
          allowedMethods: ['GET', 'POST', 'OPTIONS']
          allowedHeaders: ['*']
        }
      }
    }
    template: {
      containers: [
        {
          name: 'customer-service'
          image: images.customerService
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'Services__InsuranceService'
              value: 'http://${insuranceService.name}'
            }
            {
              name: 'Services__VehicleService'
              value: 'http://${vehicleService.name}'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

// Outputs
output customerServiceUrl string = 'https://${customerService.properties.configuration.ingress.fqdn}'
output containerAppEnvironmentId string = containerAppEnv.id
