// ==================================================================================================
// eShop Azure Infrastructure - Simplified Bicep Template
// ==================================================================================================
// This template deploys the complete Azure infrastructure for the eShop microservices application
// using standard Azure resources for immediate deployment without AVM dependencies.

@description('The name of the environment (e.g., dev, staging, prod)')
param environmentName string = 'dev'

@description('The primary Azure region where resources will be deployed')
param location string = resourceGroup().location

@description('Name prefix for all resources to ensure uniqueness')
param namePrefix string = 'grsch'

@description('The container image tag to deploy')
param imageTag string = 'latest'

@description('The Azure Container Registry login server (optional)')
param acrLoginServer string = ''

@description('Whether to enable OpenAI integration')
param enableOpenAI bool = false

@description('The SKU for PostgreSQL Flexible Server')
@allowed(['Burstable', 'GeneralPurpose', 'MemoryOptimized'])
param postgreSqlSkuTier string = 'Burstable'

@description('The compute size for PostgreSQL Flexible Server')
param postgreSqlSkuName string = 'Standard_B1ms'

@description('Whether to enable zone redundancy for production workloads')
param enableZoneRedundancy bool = false

@secure()
@description('PostgreSQL administrator password')
param postgreSqlAdminPassword string

// ==================================================================================================
// Variables
// ==================================================================================================

var resourceNames = {
  // Core infrastructure
  containerAppsEnvironment: '${namePrefix}-cae-${environmentName}'
  logAnalyticsWorkspace: '${namePrefix}-law-${environmentName}'
  applicationInsights: '${namePrefix}-ai-${environmentName}'
  
  // Container registry
  containerRegistry: '${namePrefix}acr${environmentName}${uniqueString(resourceGroup().id)}'
  
  // Database and storage
  postgreSqlServer: '${namePrefix}-postgres-${environmentName}'
  storageAccount: '${namePrefix}st${environmentName}${uniqueString(resourceGroup().id)}'
  
  // Caching and messaging
  redisCache: '${namePrefix}-redis-${environmentName}'
  serviceBus: '${namePrefix}-sb-${environmentName}'
  
  // Security
  keyVault: '${take(namePrefix, 8)}kv${take(environmentName, 3)}${take(uniqueString(resourceGroup().id), 10)}'
  managedIdentity: '${namePrefix}-mi-${environmentName}'
  
  // AI services
  openAIAccount: '${namePrefix}-openai-${environmentName}'
}

var databases = {
  catalog: 'catalogdb'
  identity: 'identitydb'
  ordering: 'orderingdb'
  webhooks: 'webhooksdb'
}

var serviceBusQueues = [
  'catalog.items.price.changed'
  'catalog.items.stock.changed'
  'ordering.order.created'
  'ordering.order.paid'
  'ordering.order.shipped'
  'ordering.order.cancelled'
  'basket.checkout.submitted'
  'payment.processing.started'
  'payment.processing.completed'
  'payment.processing.failed'
]


// ==================================================================================================
// Shared Resources
// ==================================================================================================

// Log Analytics Workspace
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: resourceNames.logAnalyticsWorkspace
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: environmentName == 'prod' ? 90 : 30
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
  }
}

// Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: resourceNames.applicationInsights
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    IngestionMode: 'LogAnalytics'
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
  }
}

// Managed Identity
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: resourceNames.managedIdentity
  location: location
  tags: {
    Environment: environmentName
    Application: 'eShop'
  }
}

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: resourceNames.keyVault
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenant().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: environmentName == 'prod' ? 90 : 7
    enablePurgeProtection: true
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
  }
}

// Container Registry
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = if (empty(acrLoginServer)) {
  name: resourceNames.containerRegistry
  location: location
  sku: {
    name: environmentName == 'prod' ? 'Premium' : 'Basic'
  }
  properties: {
    adminUserEnabled: true
    publicNetworkAccess: 'Enabled'
    zoneRedundancy: (environmentName == 'prod' && enableZoneRedundancy) ? 'Enabled' : 'Disabled'
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
  }
}

// Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: resourceNames.storageAccount
  location: location
  sku: {
    name: environmentName == 'prod' ? 'Standard_ZRS' : 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
  }
}

// ==================================================================================================
// Database Resources
// ==================================================================================================

// PostgreSQL Flexible Server
resource postgreSqlServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: resourceNames.postgreSqlServer
  location: location
  sku: {
    name: postgreSqlSkuName
    tier: postgreSqlSkuTier
  }
  properties: {
    version: '15'
    administratorLogin: 'eshop_admin'
    administratorLoginPassword: postgreSqlAdminPassword
    storage: {
      storageSizeGB: environmentName == 'prod' ? 512 : 32
      autoGrow: 'Enabled'
    }
    backup: {
      backupRetentionDays: environmentName == 'prod' ? 35 : 7
      geoRedundantBackup: environmentName == 'prod' ? 'Enabled' : 'Disabled'
    }
    highAvailability: {
      mode: (environmentName == 'prod' && enableZoneRedundancy) ? 'ZoneRedundant' : 'Disabled'
    }
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Enabled'
    }
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
  }
}

// PostgreSQL Firewall Rule - Allow Azure Services
resource postgreSqlFirewallRule 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = {
  parent: postgreSqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// PostgreSQL Extensions - pgvector is enabled at database level, not shared_preload_libraries
// The vector extension will be created via SQL: CREATE EXTENSION IF NOT EXISTS vector;

// PostgreSQL Databases
resource catalogDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgreSqlServer
  name: databases.catalog
  properties: {
    charset: 'UTF8'
    collation: 'en_US.UTF8'
  }
}

resource identityDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgreSqlServer
  name: databases.identity
  properties: {
    charset: 'UTF8'
    collation: 'en_US.UTF8'
  }
}

resource orderingDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgreSqlServer
  name: databases.ordering
  properties: {
    charset: 'UTF8'
    collation: 'en_US.UTF8'
  }
}

resource webhooksDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgreSqlServer
  name: databases.webhooks
  properties: {
    charset: 'UTF8'
    collation: 'en_US.UTF8'
  }
}

// ==================================================================================================
// Caching and Messaging
// ==================================================================================================

// Redis Cache
resource redisCache 'Microsoft.Cache/redis@2023-08-01' = {
  name: resourceNames.redisCache
  location: location
  properties: {
    sku: {
      name: environmentName == 'prod' ? 'Premium' : 'Basic'
      family: environmentName == 'prod' ? 'P' : 'C'
      capacity: environmentName == 'prod' ? 1 : 0
    }
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    redisConfiguration: {
      'maxmemory-policy': 'allkeys-lru'
    }
    redisVersion: '6'
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
  }
}

// Service Bus Namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: resourceNames.serviceBus
  location: location
  sku: {
    name: environmentName == 'prod' ? 'Standard' : 'Basic'
    tier: environmentName == 'prod' ? 'Standard' : 'Basic'
  }
  properties: {
    minimumTlsVersion: '1.2'
    // Note: premiumMessagingPartitions and zoneRedundant are only valid for Premium SKU
    // Basic and Standard SKUs don't support these properties
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
  }
}

// Service Bus Queues
resource serviceBusQueuesResource 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = [for queueName in serviceBusQueues: {
  parent: serviceBusNamespace
  name: queueName
  properties: {
    defaultMessageTimeToLive: 'P14D'
    deadLetteringOnMessageExpiration: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    enablePartitioning: false
    maxDeliveryCount: 10
    maxMessageSizeInKilobytes: 256
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
  }
}]

// ==================================================================================================
// AI Services
// ==================================================================================================

// Azure OpenAI (optional)
resource openAIAccount 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = if (enableOpenAI) {
  name: resourceNames.openAIAccount
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: resourceNames.openAIAccount
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
    }
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
  }
}

// ==================================================================================================
// Container Apps Environment
// ==================================================================================================

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: resourceNames.containerAppsEnvironment
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    zoneRedundant: (environmentName == 'prod' && enableZoneRedundancy)
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
  }
}

// ==================================================================================================
// Container Apps
// ==================================================================================================

// Catalog API
resource catalogApi 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'catalog-api'
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      registries: empty(acrLoginServer) ? [
        {
          server: '${resourceNames.containerRegistry}.azurecr.io'
          identity: managedIdentity.id
        }
      ] : [
        {
          server: acrLoginServer
          identity: managedIdentity.id
        }
      ]
      secrets: [
        {
          name: 'postgres-connection'
          value: 'Host=${postgreSqlServer.properties.fullyQualifiedDomainName};Database=${databases.catalog};Username=eshop_admin;Password=${postgreSqlAdminPassword};SSL Mode=Require;'
        }
        {
          name: 'redis-connection'
          value: '${redisCache.properties.hostName}:${redisCache.properties.sslPort},password=${redisCache.listKeys().primaryKey},ssl=True,abortConnect=False'
        }
        {
          name: 'servicebus-connection'
          value: serviceBusNamespace.listKeys().primaryConnectionString
        }
        {
          name: 'appinsights-key'
          value: applicationInsights.properties.InstrumentationKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'catalog-api'
          image: empty(acrLoginServer) ? '${resourceNames.containerRegistry}.azurecr.io/eshop/catalog-api:${imageTag}' : '${acrLoginServer}/eshop/catalog-api:${imageTag}'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environmentName == 'prod' ? 'Production' : 'Development'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'postgres-connection'
            }
            {
              name: 'ConnectionStrings__Redis'
              secretRef: 'redis-connection'
            }
            {
              name: 'EventBus__ConnectionString'
              secretRef: 'servicebus-connection'
            }
            {
              name: 'ApplicationInsights__InstrumentationKey'
              secretRef: 'appinsights-key'
            }
          ]
        }
      ]
      scale: {
        minReplicas: environmentName == 'prod' ? 2 : 1
        maxReplicas: environmentName == 'prod' ? 10 : 3
        rules: [
          {
            name: 'http-requests'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
    Service: 'catalog-api'
  }
}

// Basket API
resource basketApi 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'basket-api'
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      registries: empty(acrLoginServer) ? [
        {
          server: '${resourceNames.containerRegistry}.azurecr.io'
          identity: managedIdentity.id
        }
      ] : [
        {
          server: acrLoginServer
          identity: managedIdentity.id
        }
      ]
      secrets: [
        {
          name: 'redis-connection'
          value: '${redisCache.properties.hostName}:${redisCache.properties.sslPort},password=${redisCache.listKeys().primaryKey},ssl=True,abortConnect=False'
        }
        {
          name: 'servicebus-connection'
          value: serviceBusNamespace.listKeys().primaryConnectionString
        }
        {
          name: 'appinsights-key'
          value: applicationInsights.properties.InstrumentationKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'basket-api'
          image: empty(acrLoginServer) ? '${resourceNames.containerRegistry}.azurecr.io/eshop/basket-api:${imageTag}' : '${acrLoginServer}/eshop/basket-api:${imageTag}'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environmentName == 'prod' ? 'Production' : 'Development'
            }
            {
              name: 'ConnectionStrings__Redis'
              secretRef: 'redis-connection'
            }
            {
              name: 'EventBus__ConnectionString'
              secretRef: 'servicebus-connection'
            }
            {
              name: 'ApplicationInsights__InstrumentationKey'
              secretRef: 'appinsights-key'
            }
          ]
        }
      ]
      scale: {
        minReplicas: environmentName == 'prod' ? 2 : 1
        maxReplicas: environmentName == 'prod' ? 10 : 3
        rules: [
          {
            name: 'http-requests'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
    Service: 'basket-api'
  }
}

// Identity API
resource identityApi 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'identity-api'
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      registries: empty(acrLoginServer) ? [
        {
          server: '${resourceNames.containerRegistry}.azurecr.io'
          identity: managedIdentity.id
        }
      ] : [
        {
          server: acrLoginServer
          identity: managedIdentity.id
        }
      ]
      secrets: [
        {
          name: 'postgres-connection'
          value: 'Host=${postgreSqlServer.properties.fullyQualifiedDomainName};Database=${databases.identity};Username=eshop_admin;Password=${postgreSqlAdminPassword};SSL Mode=Require;'
        }
        {
          name: 'appinsights-key'
          value: applicationInsights.properties.InstrumentationKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'identity-api'
          image: empty(acrLoginServer) ? '${resourceNames.containerRegistry}.azurecr.io/eshop/identity-api:${imageTag}' : '${acrLoginServer}/eshop/identity-api:${imageTag}'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environmentName == 'prod' ? 'Production' : 'Development'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'postgres-connection'
            }
            {
              name: 'ApplicationInsights__InstrumentationKey'
              secretRef: 'appinsights-key'
            }
          ]
        }
      ]
      scale: {
        minReplicas: environmentName == 'prod' ? 2 : 1
        maxReplicas: environmentName == 'prod' ? 10 : 3
        rules: [
          {
            name: 'http-requests'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
    Service: 'identity-api'
  }
  dependsOn: [
    containerRegistry
  ]
}

// Ordering API
resource orderingApi 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'ordering-api'
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      registries: empty(acrLoginServer) ? [
        {
          server: '${resourceNames.containerRegistry}.azurecr.io'
          identity: managedIdentity.id
        }
      ] : [
        {
          server: acrLoginServer
          identity: managedIdentity.id
        }
      ]
      secrets: [
        {
          name: 'postgres-connection'
          value: 'Host=${postgreSqlServer.properties.fullyQualifiedDomainName};Database=${databases.ordering};Username=eshop_admin;Password=${postgreSqlAdminPassword};SSL Mode=Require;'
        }
        {
          name: 'servicebus-connection'
          value: serviceBusNamespace.listKeys().primaryConnectionString
        }
        {
          name: 'appinsights-key'
          value: applicationInsights.properties.InstrumentationKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'ordering-api'
          image: empty(acrLoginServer) ? '${resourceNames.containerRegistry}.azurecr.io/eshop/ordering-api:${imageTag}' : '${acrLoginServer}/eshop/ordering-api:${imageTag}'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environmentName == 'prod' ? 'Production' : 'Development'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'postgres-connection'
            }
            {
              name: 'EventBus__ConnectionString'
              secretRef: 'servicebus-connection'
            }
            {
              name: 'ApplicationInsights__InstrumentationKey'
              secretRef: 'appinsights-key'
            }
          ]
        }
      ]
      scale: {
        minReplicas: environmentName == 'prod' ? 2 : 1
        maxReplicas: environmentName == 'prod' ? 10 : 3
        rules: [
          {
            name: 'http-requests'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
    Service: 'ordering-api'
  }
  dependsOn: [
    containerRegistry
  ]
}

// Payment Processor
resource paymentProcessor 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'payment-processor'
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
      }
      registries: empty(acrLoginServer) ? [
        {
          server: '${resourceNames.containerRegistry}.azurecr.io'
          identity: managedIdentity.id
        }
      ] : [
        {
          server: acrLoginServer
          identity: managedIdentity.id
        }
      ]
      secrets: [
        {
          name: 'servicebus-connection'
          value: serviceBusNamespace.listKeys().primaryConnectionString
        }
        {
          name: 'appinsights-key'
          value: applicationInsights.properties.InstrumentationKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'payment-processor'
          image: empty(acrLoginServer) ? '${resourceNames.containerRegistry}.azurecr.io/eshop/payment-processor:${imageTag}' : '${acrLoginServer}/eshop/payment-processor:${imageTag}'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environmentName == 'prod' ? 'Production' : 'Development'
            }
            {
              name: 'EventBus__ConnectionString'
              secretRef: 'servicebus-connection'
            }
            {
              name: 'ApplicationInsights__InstrumentationKey'
              secretRef: 'appinsights-key'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: environmentName == 'prod' ? 5 : 2
        rules: [
          {
            name: 'servicebus-messages'
            custom: {
              type: 'azure-servicebus'
              metadata: {
                connectionFromEnv: 'EventBus__ConnectionString'
                queueName: 'payment.processing.started'
                messageCount: '10'
              }
            }
          }
        ]
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
    Service: 'payment-processor'
  }
  dependsOn: [
    containerRegistry
  ]
}

// Order Processor
resource orderProcessor 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'order-processor'
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
      }
      registries: empty(acrLoginServer) ? [
        {
          server: '${resourceNames.containerRegistry}.azurecr.io'
          identity: managedIdentity.id
        }
      ] : [
        {
          server: acrLoginServer
          identity: managedIdentity.id
        }
      ]
      secrets: [
        {
          name: 'postgres-connection'
          value: 'Host=${postgreSqlServer.properties.fullyQualifiedDomainName};Database=${databases.ordering};Username=eshop_admin;Password=${postgreSqlAdminPassword};SSL Mode=Require;'
        }
        {
          name: 'servicebus-connection'
          value: serviceBusNamespace.listKeys().primaryConnectionString
        }
        {
          name: 'appinsights-key'
          value: applicationInsights.properties.InstrumentationKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'order-processor'
          image: empty(acrLoginServer) ? '${resourceNames.containerRegistry}.azurecr.io/eshop/order-processor:${imageTag}' : '${acrLoginServer}/eshop/order-processor:${imageTag}'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environmentName == 'prod' ? 'Production' : 'Development'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'postgres-connection'
            }
            {
              name: 'EventBus__ConnectionString'
              secretRef: 'servicebus-connection'
            }
            {
              name: 'ApplicationInsights__InstrumentationKey'
              secretRef: 'appinsights-key'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: environmentName == 'prod' ? 5 : 2
        rules: [
          {
            name: 'servicebus-messages'
            custom: {
              type: 'azure-servicebus'
              metadata: {
                connectionFromEnv: 'EventBus__ConnectionString'
                queueName: 'ordering.order.created'
                messageCount: '10'
              }
            }
          }
        ]
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
    Service: 'order-processor'
  }
  dependsOn: [
    containerRegistry
  ]
}

// Webhooks API
resource webhooksApi 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'webhooks-api'
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      registries: empty(acrLoginServer) ? [
        {
          server: '${resourceNames.containerRegistry}.azurecr.io'
          identity: managedIdentity.id
        }
      ] : [
        {
          server: acrLoginServer
          identity: managedIdentity.id
        }
      ]
      secrets: [
        {
          name: 'postgres-connection'
          value: 'Host=${postgreSqlServer.properties.fullyQualifiedDomainName};Database=${databases.webhooks};Username=eshop_admin;Password=${postgreSqlAdminPassword};SSL Mode=Require;'
        }
        {
          name: 'servicebus-connection'
          value: serviceBusNamespace.listKeys().primaryConnectionString
        }
        {
          name: 'appinsights-key'
          value: applicationInsights.properties.InstrumentationKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'webhooks-api'
          image: empty(acrLoginServer) ? '${resourceNames.containerRegistry}.azurecr.io/eshop/webhooks-api:${imageTag}' : '${acrLoginServer}/eshop/webhooks-api:${imageTag}'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environmentName == 'prod' ? 'Production' : 'Development'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'postgres-connection'
            }
            {
              name: 'EventBus__ConnectionString'
              secretRef: 'servicebus-connection'
            }
            {
              name: 'ApplicationInsights__InstrumentationKey'
              secretRef: 'appinsights-key'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: environmentName == 'prod' ? 3 : 2
        rules: [
          {
            name: 'http-requests'
            http: {
              metadata: {
                concurrentRequests: '50'
              }
            }
          }
        ]
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
    Service: 'webhooks-api'
  }
  dependsOn: [
    containerRegistry
  ]
}

// Web App (Frontend)
resource webApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'webapp'
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      registries: empty(acrLoginServer) ? [
        {
          server: '${resourceNames.containerRegistry}.azurecr.io'
          identity: managedIdentity.id
        }
      ] : [
        {
          server: acrLoginServer
          identity: managedIdentity.id
        }
      ]
      secrets: [
        {
          name: 'appinsights-key'
          value: applicationInsights.properties.InstrumentationKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'webapp'
          image: empty(acrLoginServer) ? '${resourceNames.containerRegistry}.azurecr.io/eshop/webapp:${imageTag}' : '${acrLoginServer}/eshop/webapp:${imageTag}'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environmentName == 'prod' ? 'Production' : 'Development'
            }
            {
              name: 'IdentityUrl'
              value: 'https://${identityApi.properties.configuration.ingress.fqdn}'
            }
            {
              name: 'CatalogUrl'
              value: 'https://${catalogApi.properties.configuration.ingress.fqdn}'
            }
            {
              name: 'OrderingUrl'
              value: 'https://${orderingApi.properties.configuration.ingress.fqdn}'
            }
            {
              name: 'BasketUrl'
              value: 'https://${basketApi.properties.configuration.ingress.fqdn}'
            }
            {
              name: 'ApplicationInsights__InstrumentationKey'
              secretRef: 'appinsights-key'
            }
          ]
        }
      ]
      scale: {
        minReplicas: environmentName == 'prod' ? 2 : 1
        maxReplicas: environmentName == 'prod' ? 10 : 3
        rules: [
          {
            name: 'http-requests'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  tags: {
    Environment: environmentName
    Application: 'eShop'
    Service: 'webapp'
  }
  dependsOn: [
    containerRegistry
  ]
}

// ==================================================================================================
// Role Assignments
// ==================================================================================================

// Key Vault Secrets Officer role for Managed Identity
resource keyVaultSecretsOfficerRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
}

resource managedIdentityKeyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(keyVault.id, managedIdentity.id, keyVaultSecretsOfficerRole.id)
  properties: {
    roleDefinitionId: keyVaultSecretsOfficerRole.id
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ACR Pull role for Managed Identity (if using our own ACR)
resource acrPullRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '7f951dda-4ed3-4680-a7ca-43fe172d538d'
}

resource managedIdentityAcrRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (empty(acrLoginServer)) {
  scope: containerRegistry
  name: guid(containerRegistry.id, managedIdentity.id, acrPullRole.id)
  properties: {
    roleDefinitionId: acrPullRole.id
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ==================================================================================================
// Outputs
// ==================================================================================================

@description('The resource group name where resources were deployed')
output resourceGroupName string = resourceGroup().name

@description('The name of the Container Apps environment')
output containerAppsEnvironmentName string = containerAppsEnvironment.name

@description('The FQDN of the web application')
output webAppUrl string = 'https://${webApp.properties.configuration.ingress.fqdn}'

@description('The URLs of the API endpoints')
output apiEndpoints object = {
  catalog: 'https://${catalogApi.properties.configuration.ingress.fqdn}'
  basket: 'https://${basketApi.properties.configuration.ingress.fqdn}'
  identity: 'https://${identityApi.properties.configuration.ingress.fqdn}'
  ordering: 'https://${orderingApi.properties.configuration.ingress.fqdn}'
  webhooks: 'https://${webhooksApi.properties.configuration.ingress.fqdn}'
}

@description('The PostgreSQL server details')
output postgreSqlServer object = {
  serverName: postgreSqlServer.name
  fullyQualifiedDomainName: postgreSqlServer.properties.fullyQualifiedDomainName
  databases: databases
}

@description('The Redis cache details')
output redisCache object = {
  name: redisCache.name
  hostName: redisCache.properties.hostName
  sslPort: redisCache.properties.sslPort
}

@description('The Service Bus namespace details')
output serviceBus object = {
  namespaceName: serviceBusNamespace.name
  queues: serviceBusQueues
}

@description('The Container Registry details (if created)')
output containerRegistry object = empty(acrLoginServer) ? {
  name: containerRegistry.name
  loginServer: '${resourceNames.containerRegistry}.azurecr.io'
} : {
  name: 'external'
  loginServer: acrLoginServer
}

@description('The Application Insights details')
output applicationInsights object = {
  name: applicationInsights.name
  instrumentationKey: applicationInsights.properties.InstrumentationKey
  connectionString: applicationInsights.properties.ConnectionString
}

@description('The Key Vault details')
output keyVault object = {
  name: keyVault.name
  vaultUri: keyVault.properties.vaultUri
}

@description('The Managed Identity details')
output managedIdentity object = {
  name: managedIdentity.name
  principalId: managedIdentity.properties.principalId
  clientId: managedIdentity.properties.clientId
}

@description('OpenAI account details (if enabled)')
output openAIAccount object = enableOpenAI ? {
  name: openAIAccount.name
  endpoint: 'https://${resourceNames.openAIAccount}.openai.azure.com/'
} : {}
