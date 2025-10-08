// ==================================================================================================
// eShop Azure Infrastructure - Parameters File
// ==================================================================================================
// This parameters file provides default values for the eShop infrastructure deployment.
// Customize these values based on your specific deployment requirements.

using './main.bicep'

// ==================================================================================================
// Core Parameters
// ==================================================================================================

@description('The name of the environment (dev, staging, prod)')
param environmentName = 'dev'

@description('Name prefix for all resources to ensure uniqueness')
param namePrefix = 'grsch'

@description('The container image tag to deploy')
param imageTag = 'latest'

@description('The Azure Container Registry login server (leave empty to create new ACR)')
param acrLoginServer = ''

// ==================================================================================================
// Feature Flags
// ==================================================================================================

@description('Whether to enable OpenAI integration')
param enableOpenAI = false

@description('Whether to enable zone redundancy for production workloads')
param enableZoneRedundancy = false

// ==================================================================================================
// Database Configuration
// ==================================================================================================

@description('The SKU tier for PostgreSQL Flexible Server')
param postgreSqlSkuTier = 'Burstable'

@description('The compute size for PostgreSQL Flexible Server')
param postgreSqlSkuName = 'Standard_B1ms'

@description('PostgreSQL administrator password')
param postgreSqlAdminPassword = 'eShop123!@#'
