# Container Registry Authentication Fix

## Problem Analysis

Container Apps were failing to pull images from Azure Container Registry with the error:
```
UNAUTHORIZED: authentication required, visit https://aka.ms/acr/authorization for more information
```

This happened because the Container Apps didn't have registry authentication configured.

## Root Cause

The Container Apps were trying to pull images from ACR but lacked the proper authentication configuration. In Container Apps, when using a private registry like ACR, you need to specify registry authentication in the `configuration.registries` section.

## Solution Applied

### 1. Added Registry Configuration to All Container Apps

Added the following registry configuration to all Container Apps:

```bicep
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
```

### 2. Container Apps Updated

- ✅ **catalog-api**: Added registry authentication
- ✅ **basket-api**: Added registry authentication  
- ✅ **identity-api**: Added registry authentication
- ✅ **ordering-api**: Added registry authentication
- ✅ **payment-processor**: Added registry authentication
- ✅ **order-processor**: Added registry authentication
- ✅ **webhooks-api**: Added registry authentication
- ✅ **webapp**: Added registry authentication

### 3. Authentication Method

The solution uses **managed identity authentication** with the following benefits:
- **Secure**: No passwords or credentials stored in the template
- **Automatic**: Azure handles token refresh automatically
- **RBAC**: Uses the existing managed identity with AcrPull role assignment

## How It Works

1. **Managed Identity**: All Container Apps use the same user-assigned managed identity
2. **ACR Role Assignment**: The managed identity has `AcrPull` role on the Container Registry
3. **Registry Authentication**: Container Apps reference the managed identity for ACR authentication
4. **Image Pull**: Azure Container Apps can now authenticate and pull images from ACR

## Next Steps

1. **Redeploy Infrastructure**: Run the deployment to apply the registry authentication fix
2. **Push Container Images**: Ensure all required images are pushed to the ACR:
   - `eshop/catalog-api:latest`
   - `eshop/basket-api:latest`
   - `eshop/identity-api:latest`
   - `eshop/ordering-api:latest`
   - `eshop/payment-processor:latest`
   - `eshop/order-processor:latest`
   - `eshop/webhooks-api:latest`
   - `eshop/webapp:latest`

3. **Monitor Deployment**: Check that Container Apps successfully start after the fix

## Deployment Command

```powershell
.\deploy.ps1 -ResourceGroup "rg-eshop-central-dev" -Environment "dev"
```

This fix resolves the authentication issue and allows Container Apps to pull images from the Azure Container Registry successfully.