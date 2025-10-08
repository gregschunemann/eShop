# eShop Azure Infrastructure

This directory contains the Azure infrastructure as code (Bicep) templates for deploying the eShop microservices application to Azure.

## Architecture Overview

The eShop solution is deployed using Azure Container Apps and includes the following components:

### Core Infrastructure
- **Azure Container Apps Environment** - Hosting environment for all microservices
- **Log Analytics Workspace** - Centralized logging and monitoring
- **Application Insights** - Application performance monitoring
- **Azure Key Vault** - Secure storage for secrets and configuration
- **Managed Identity** - Secure access to Azure resources

### Data Services
- **PostgreSQL Flexible Server** - Primary database with multiple databases for different services
- **Azure Redis Cache** - Session storage and caching
- **Azure Storage Account** - Blob storage for catalog images and file uploads

### Messaging
- **Azure Service Bus** - Event-driven communication between microservices

### Application Services
- **Identity API** - Authentication and authorization service
- **Catalog API** - Product catalog management
- **Basket API** - Shopping cart functionality
- **Ordering API** - Order processing
- **Webhooks API** - Webhook management
- **Web App** - Main e-commerce frontend
- **Mobile BFF** - Backend-for-frontend for mobile clients
- **Background Services** - Order and payment processors

### Optional Services
- **Azure OpenAI** - AI-powered features (text embedding and chat)

## Deployment

### Prerequisites

1. Azure CLI installed and logged in
2. Bicep CLI installed
3. Sufficient permissions in your Azure subscription

### Quick Deployment

1. **Clone the repository and navigate to the infrastructure directory:**
   ```bash
   cd infra
   ```

2. **Create a resource group:**
   ```bash
   az group create --name "rg-eshop-dev" --location "East US"
   ```

3. **Deploy the infrastructure:**
   ```bash
   az deployment group create \
     --resource-group "rg-eshop-dev" \
     --template-file main.bicep \
     --parameters main.bicepparam
   ```

### Custom Deployment

1. **Modify the parameters file** (`main.bicepparam`) with your desired configuration:
   - Change `environmentName` for different environments (dev, staging, prod)
   - Update `namePrefix` to ensure resource name uniqueness
   - Configure database sizing based on your needs
   - Enable/disable optional features like OpenAI

2. **Deploy with custom parameters:**
   ```bash
   az deployment group create \
     --resource-group "your-resource-group" \
     --template-file main.bicep \
     --parameters @main.bicepparam \
     --parameters environmentName=prod enableZoneRedundancy=true
   ```

### Environment-Specific Deployments

For different environments, you can create environment-specific parameter files:

**Development Environment (`dev.bicepparam`):**
```bicep
using './main.bicep'

param environmentName = 'dev'
param namePrefix = 'eshop'
param postgreSqlSkuTier = 'Burstable'
param postgreSqlSkuName = 'Standard_B1ms'
param enableZoneRedundancy = false
param enableOpenAI = false
```

**Production Environment (`prod.bicepparam`):**
```bicep
using './main.bicep'

param environmentName = 'prod'
param namePrefix = 'eshop'
param postgreSqlSkuTier = 'GeneralPurpose'
param postgreSqlSkuName = 'Standard_D2ds_v4'
param enableZoneRedundancy = true
param enableOpenAI = true
```

### Container Images

The template assumes container images are available in either:
1. An existing Azure Container Registry (specify `acrLoginServer` parameter)
2. A new Azure Container Registry (created by the template if `acrLoginServer` is empty)

Container image names should follow this pattern:
- `identity-api:latest`
- `catalog-api:latest`
- `basket-api:latest`
- `ordering-api:latest`
- `order-processor:latest`
- `payment-processor:latest`
- `webhooks-api:latest`
- `webhooks-client:latest`
- `webapp:latest`
- `mobile-bff:latest`

## Configuration

### Database Configuration

The template creates a PostgreSQL Flexible Server with the following databases:
- `catalogdb` - Catalog API database
- `identitydb` - Identity API database
- `orderingdb` - Ordering API database
- `webhooksdb` - Webhooks API database

### Security

- All container apps use managed identity for secure access to Azure resources
- Database connections use temporary passwords (should be rotated after deployment)
- Secrets are stored in Azure Key Vault
- SSL/TLS is enforced for all external connections

### Scaling

The template includes horizontal pod autoscaling based on:
- HTTP request volume
- Service Bus queue length (for background services)

Default scaling parameters:
- Web applications: 1-10 replicas
- APIs: 1-10 replicas
- Background services: 0-5 replicas

### Monitoring

- Application Insights integration for all services
- Log Analytics workspace for centralized logging
- Health checks configured for all container apps

## Post-Deployment Steps

1. **Rotate database passwords** stored in Key Vault
2. **Configure DNS** (if using custom domains)
3. **Set up CI/CD pipelines** for container image deployment
4. **Configure backup policies** for databases
5. **Set up monitoring alerts** based on your requirements

## Costs

Estimated monthly costs for different environments:

**Development:**
- Container Apps: ~$30-50
- PostgreSQL: ~$15-25
- Redis: ~$15-20
- Other services: ~$20-30
- **Total: ~$80-125/month**

**Production (with zone redundancy):**
- Container Apps: ~$200-400
- PostgreSQL: ~$150-300
- Redis: ~$50-100
- Other services: ~$100-200
- **Total: ~$500-1000/month**

*Costs may vary based on usage patterns and regional pricing.*

## Troubleshooting

### Common Issues

1. **Container Apps not starting:**
   - Check container images are available in the registry
   - Verify database connections
   - Review Application Insights logs

2. **Database connection failures:**
   - Ensure firewall rules allow Container Apps
   - Check credentials in Key Vault
   - Verify database server is running

3. **Service Bus connection issues:**
   - Verify managed identity has proper permissions
   - Check Service Bus namespace configuration

### Useful Commands

```bash
# Check deployment status
az deployment group show --resource-group "rg-eshop-dev" --name "main"

# View container app logs
az containerapp logs show --name "eshop-webapp-dev" --resource-group "rg-eshop-dev"

# Scale a container app
az containerapp update --name "eshop-webapp-dev" --resource-group "rg-eshop-dev" --min-replicas 2 --max-replicas 20

# Check database status
az postgres flexible-server show --name "eshop-postgres-dev" --resource-group "rg-eshop-dev"
```

## Support

For issues related to the infrastructure templates, please check:
1. Azure Container Apps documentation
2. Bicep language reference
3. Azure CLI troubleshooting guides