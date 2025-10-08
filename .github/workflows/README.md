# GitHub Actions Setup for eShop Infrastructure

This document explains how to configure GitHub Actions for automated deployment of the eShop infrastructure to Azure.

## Required Secrets and Variables

### GitHub Secrets

The following secrets need to be configured in your repository settings (Settings → Secrets and variables → Actions):

#### 1. AZURE_CREDENTIALS
Create a service principal for GitHub Actions authentication:

```bash
# Login to Azure
az login

# Create service principal
az ad sp create-for-rbac --name "github-actions-eshop" \
  --role contributor \
  --scopes /subscriptions/{subscription-id} \
  --sdk-auth

# Output will be something like:
{
  "clientId": "...",
  "clientSecret": "...",
  "subscriptionId": "...",
  "tenantId": "...",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

Copy the entire JSON output and add it as the `AZURE_CREDENTIALS` secret.

#### 2. OpenAI Integration Secrets (Optional)
If you want to enable OpenAI integration:

- `OPENAI_ENDPOINT`: Your Azure OpenAI endpoint URL
- `OPENAI_API_KEY`: Your Azure OpenAI API key

### GitHub Variables

Configure these in Settings → Secrets and variables → Actions → Variables:

#### 1. CONTAINER_REGISTRY_SERVER (Optional)
If you have an existing Azure Container Registry:
- Variable: `CONTAINER_REGISTRY_SERVER`
- Value: `yourregistry.azurecr.io`

## Workflow Configuration

### Environment-Based Deployments

The workflow supports multiple deployment strategies:

1. **Branch-based automatic deployment:**
   - `main` branch → Production environment
   - `develop` branch → Staging environment
   - Other branches → Development environment

2. **Manual deployment:**
   - Use workflow_dispatch to deploy to any environment
   - Choose environment: dev, staging, or prod
   - Toggle features: OpenAI integration, Zone redundancy

### Environments

Configure deployment environments in GitHub (Settings → Environments):

1. **dev** - Development environment
   - No protection rules needed
   - Auto-deployment on feature branches

2. **staging** - Staging environment
   - Optional: Require reviewers
   - Auto-deployment from `develop` branch

3. **prod** - Production environment
   - **Recommended**: Require reviewers
   - **Recommended**: Deployment protection rules
   - Auto-deployment from `main` branch

## Workflow Jobs

### 1. Validate
- Lints Bicep templates
- Creates resource groups
- Validates deployment without executing

### 2. Deploy
- Deploys infrastructure using Bicep templates
- Outputs deployment URLs and connection strings
- Only runs on push to main branches (not on PRs)

### 3. Health Check
- Verifies all Azure resources are in healthy state
- Checks Container Apps, PostgreSQL, Redis, and Service Bus
- Creates deployment summary

## Usage Examples

### Automatic Deployment
Push to any branch to trigger validation. Push to `main` or `develop` to deploy:

```bash
git checkout main
git push origin main  # Deploys to production
```

```bash
git checkout develop
git push origin develop  # Deploys to staging
```

### Manual Deployment
1. Go to Actions tab in GitHub
2. Select "Deploy eShop Infrastructure"
3. Click "Run workflow"
4. Choose environment and features
5. Click "Run workflow"

### Pull Request Validation
Create a PR to validate infrastructure changes without deployment:

```bash
git checkout -b feature/infrastructure-update
# Make changes to infra/ folder
git commit -am "Update infrastructure"
git push origin feature/infrastructure-update
# Create PR - validation will run automatically
```

## Security Best Practices

### Service Principal Permissions
Grant minimal required permissions to the service principal:

```bash
# Option 1: Contributor role on specific resource groups
az role assignment create \
  --assignee {service-principal-id} \
  --role Contributor \
  --scope /subscriptions/{subscription-id}/resourceGroups/rg-eshop-dev

az role assignment create \
  --assignee {service-principal-id} \
  --role Contributor \
  --scope /subscriptions/{subscription-id}/resourceGroups/rg-eshop-staging

az role assignment create \
  --assignee {service-principal-id} \
  --role Contributor \
  --scope /subscriptions/{subscription-id}/resourceGroups/rg-eshop-prod

# Option 2: Custom role with specific permissions
az role definition create --role-definition '{
  "Name": "eShop Infrastructure Deployer",
  "Description": "Can deploy eShop infrastructure",
  "Actions": [
    "Microsoft.Resources/*/read",
    "Microsoft.Resources/deployments/*",
    "Microsoft.App/*",
    "Microsoft.DBforPostgreSQL/*",
    "Microsoft.Cache/*",
    "Microsoft.ServiceBus/*",
    "Microsoft.ContainerRegistry/*",
    "Microsoft.OperationalInsights/*",
    "Microsoft.CognitiveServices/*"
  ],
  "AssignableScopes": ["/subscriptions/{subscription-id}"]
}'
```

### Environment Protection
For production deployments:

1. Enable required reviewers
2. Set deployment protection rules
3. Limit deployment to specific branches
4. Enable deployment logs retention

## Troubleshooting

### Common Issues

1. **Authentication Failures**
   - Verify AZURE_CREDENTIALS secret is correctly formatted JSON
   - Check service principal has correct permissions
   - Ensure subscription ID is correct

2. **Resource Group Not Found**
   - The workflow creates resource groups automatically
   - Check Azure location is supported for all resources
   - Verify subscription limits aren't exceeded

3. **Deployment Timeouts**
   - PostgreSQL and other managed services can take 10-15 minutes
   - Increase workflow timeout if needed
   - Check Azure service health status

4. **Parameter Validation Errors**
   - Review parameter validation in main.bicep
   - Check environment-specific constraints
   - Verify optional features are properly configured

### Logs and Monitoring

- **GitHub Actions logs**: Available in Actions tab
- **Azure deployment logs**: Check resource group deployments in Azure Portal
- **Application Insights**: Monitor deployed applications
- **Azure Monitor**: Infrastructure health and performance

## Cost Optimization

The infrastructure deployment includes cost optimization features:

- **Development**: Uses Basic/Standard tiers, single AZ
- **Staging**: Balanced performance and cost
- **Production**: High availability, zone redundancy, premium features

Monitor costs using:
- Azure Cost Management
- Resource group cost analysis
- Azure Advisor recommendations

## Next Steps

After successful infrastructure deployment:

1. **Deploy Applications**: Use separate workflows for container image builds and deployments
2. **Configure Monitoring**: Set up alerts and dashboards
3. **Database Setup**: Run database migrations and seed data
4. **SSL Certificates**: Configure custom domains and SSL
5. **CDN Setup**: Configure Azure CDN for static assets

## Support

For issues with this GitHub Actions workflow:
1. Check the troubleshooting section above
2. Review Azure deployment logs
3. Validate Bicep templates locally using Azure CLI
4. Consult Azure documentation for specific service issues