# eShop Simplified Bicep Template - Changes Summary

## Overview

This document summarizes the changes made to convert the eShop infrastructure from Azure Verified Modules (AVM) to standard Azure resources for immediate deployment compatibility.

## Key Changes Made

### 1. Removed Azure Verified Module Dependencies
- **Before**: Used `br/public:avm/res/...` module references
- **After**: Uses standard Azure resource declarations (`Microsoft.*/...@API-version`)
- **Benefit**: Eliminates AVM compatibility issues and registry access problems

### 2. Fixed Resource Reference Issues
- **Container Registry**: Resolved conditional deployment issues with null references
- **OpenAI Account**: Fixed optional resource property access
- **Dependencies**: Removed unnecessary `dependsOn` declarations

### 3. Simplified Resource Naming
- **Container Registry**: Uses predictable naming with `uniqueString()` for global uniqueness
- **Storage Account**: Uses predictable naming with `uniqueString()` for global uniqueness
- **Key Vault**: Uses predictable naming with `uniqueString()` for global uniqueness

### 4. Enhanced Security
- **PostgreSQL Password**: Now uses secure parameter instead of hardcoded value
- **Key Vault**: Proper RBAC configuration maintained
- **Managed Identity**: Proper role assignments for ACR and Key Vault access

### 5. Fixed Property Names
- **Key Vault**: Changed `purgeProtectionEnabled` to `enablePurgeProtection`
- **Container Apps**: Used correct property structures for current API versions

## Files Modified

### `main.bicep`
- Complete rewrite using standard Azure resources
- Removed all AVM module references
- Fixed conditional resource deployment issues
- Added proper dependency management

### `main.bicepparam`
- Added required `postgreSqlAdminPassword` parameter
- Removed unused OpenAI configuration parameters
- Maintained all other existing parameters

## Infrastructure Components

The simplified template deploys the same infrastructure as before:

### Core Infrastructure
- ✅ Container Apps Environment with Log Analytics integration
- ✅ Application Insights for monitoring
- ✅ Managed Identity for secure access
- ✅ Key Vault for secrets management

### Database & Storage
- ✅ PostgreSQL Flexible Server with vector extension
- ✅ Multiple databases (catalog, identity, ordering, webhooks)
- ✅ Storage Account for blob storage

### Caching & Messaging
- ✅ Redis Cache for session storage
- ✅ Service Bus with all required queues

### Container Services
- ✅ Azure Container Registry (optional)
- ✅ Container Apps for all microservices:
  - Catalog API
  - Basket API
  - Identity API
  - Ordering API
  - Payment Processor
  - Order Processor
  - Webhooks API
  - Web App (Frontend)

### AI Services (Optional)
- ✅ Azure OpenAI Account (when `enableOpenAI = true`)

## Deployment Instructions

### Prerequisites
1. Azure CLI or PowerShell with Az module
2. Bicep CLI installed
3. Resource Group created

### Deployment Command
```powershell
# Using the existing deploy.ps1 script
.\infra\deploy.ps1 -EnvironmentName "dev" -Location "East US 2"
```

### Parameters
All parameters are configured in `main.bicepparam`:
- `environmentName`: Environment designation (dev/staging/prod)
- `namePrefix`: Resource naming prefix
- `postgreSqlAdminPassword`: Database administrator password
- `enableOpenAI`: Optional AI services
- `enableZoneRedundancy`: High availability for production

## Benefits of Simplified Approach

1. **Immediate Deployment**: No AVM registry or version compatibility issues
2. **Predictable Naming**: Resources use consistent naming patterns
3. **Better Error Handling**: Proper conditional resource deployment
4. **Standard Resources**: Uses well-documented Azure resource providers
5. **Maintainable**: Standard Bicep syntax without external dependencies

## Post-Deployment

After successful deployment, you will receive outputs including:
- Web application URL
- API endpoint URLs
- Database connection details
- Container registry information
- Monitoring and security resource details

## Next Steps

1. Build and push container images to the created ACR
2. Update container apps with actual image references
3. Configure CI/CD pipelines using the provided GitHub Actions workflow
4. Set up monitoring alerts and dashboards
5. Configure custom domains and SSL certificates for production

## Security Notes

- The PostgreSQL password is set via parameter - change the default value
- All resources use managed identities for secure access
- Network security follows Azure best practices
- Key Vault RBAC is properly configured
- Container images should be scanned for vulnerabilities before deployment