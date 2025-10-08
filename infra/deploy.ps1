# eShop Azure Infrastructure - PowerShell Deployment Script
# ==================================================================================================
# This script deploys the eShop microservices application to Azure using Bicep templates.
# It provides options for different environments and deployment configurations.

param(
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroup,
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory = $false)]
    [string]$Location = "Central US",
    
    [Parameter(Mandatory = $false)]
    [string]$NamePrefix = "eshop",
    
    [Parameter(Mandatory = $false)]
    [string]$SubscriptionId = "",
    
    [Parameter(Mandatory = $false)]
    [switch]$EnableOpenAI,
    
    [Parameter(Mandatory = $false)]
    [switch]$EnableZoneRedundancy,
    
    [Parameter(Mandatory = $false)]
    [string]$ContainerRegistry = "",
    
    [Parameter(Mandatory = $false)]
    [string]$ImageTag = "latest",
    
    [Parameter(Mandatory = $false)]
    [string]$OpenAIEndpoint = "",
    
    [Parameter(Mandatory = $false)]
    [SecureString]$OpenAIApiKey,
    
    [Parameter(Mandatory = $false)]
    [switch]$WhatIf,
    
    [Parameter(Mandatory = $false)]
    [switch]$Help
)

# Display help if requested
if ($Help) {
    Write-Host @"
eShop Azure Deployment Script

Usage: .\deploy.ps1 -ResourceGroup <name> [OPTIONS]

Required Parameters:
  -ResourceGroup          Name of the Azure resource group

Optional Parameters:
  -Environment           Environment name (dev, staging, prod) [default: dev]
  -Location              Azure region [default: East US]
  -NamePrefix            Name prefix for resources [default: eshop]
  -SubscriptionId        Azure subscription ID
  -EnableOpenAI          Enable OpenAI integration
  -EnableZoneRedundancy  Enable zone redundancy for production
  -ContainerRegistry     Existing container registry server
  -ImageTag              Container image tag [default: latest]
  -OpenAIEndpoint        OpenAI endpoint URL (if EnableOpenAI is used)
  -OpenAIApiKey          OpenAI API key (if EnableOpenAI is used)
  -WhatIf                Preview changes without deploying
  -Help                  Show this help message

Examples:
  .\deploy.ps1 -ResourceGroup "rg-eshop-dev" -Environment "dev"
  .\deploy.ps1 -ResourceGroup "rg-eshop-prod" -Environment "prod" -EnableZoneRedundancy -EnableOpenAI
  .\deploy.ps1 -ResourceGroup "rg-eshop-staging" -Environment "staging" -ContainerRegistry "myregistry.azurecr.io"
"@
    return
}

# Color functions
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Set error action preference
$ErrorActionPreference = "Stop"

try {
    # Set subscription if provided
    if ($SubscriptionId) {
        Write-Info "Setting Azure subscription to $SubscriptionId"
        az account set --subscription $SubscriptionId
        if ($LASTEXITCODE -ne 0) { throw "Failed to set subscription" }
    }

    # Verify Azure CLI login
    Write-Info "Verifying Azure CLI login..."
    $accountInfo = az account show 2>$null | ConvertFrom-Json
    if (-not $accountInfo) {
        Write-Error "Not logged in to Azure CLI. Please run 'az login' first."
        return
    }

    Write-Info "Using Azure subscription: $($accountInfo.id)"

    # Create resource group if it doesn't exist
    Write-Info "Checking if resource group '$ResourceGroup' exists..."
    $groupExists = az group exists --name $ResourceGroup | ConvertFrom-Json
    
    if (-not $groupExists) {
        Write-Info "Creating resource group '$ResourceGroup' in '$Location'..."
        az group create --name $ResourceGroup --location $Location
        if ($LASTEXITCODE -ne 0) { throw "Failed to create resource group" }
        Write-Success "Resource group created successfully"
    } else {
        Write-Info "Resource group '$ResourceGroup' already exists"
    }

    # Note: To modify deployment parameters, edit the main.bicepparam file
    Write-Info "Using parameters from main.bicepparam file"
    Write-Warning "To modify deployment parameters (name prefix, environment, etc.), edit main.bicepparam file"

    # Show deployment summary
    Write-Host ""
    Write-Info "=== Deployment Summary ==="
    Write-Info "Resource Group: $ResourceGroup"
    Write-Info "Location: $Location"
    Write-Info "Parameters: Using main.bicepparam file"
    Write-Host ""

    # Confirm deployment unless WhatIf
    if (-not $WhatIf) {
        $confirm = Read-Host "Proceed with deployment? (y/N)"
        if ($confirm -notmatch "^[Yy]$") {
            Write-Info "Deployment cancelled"
            return
        }
    }

    # Use the bicepparam file for deployment instead of command line parameters
    Write-Info "Using main.bicepparam file for deployment parameters"

    # Validate Bicep template
    Write-Info "Validating Bicep template..."
    $validateArgs = @(
        "deployment", "group", "validate",
        "--resource-group", $ResourceGroup,
        "--template-file", "main.bicep",
        "--parameters", "main.bicepparam"
    )
    
    $validateResult = & az @validateArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Bicep template validation failed"
        Write-Host $validateResult
        return
    }
    Write-Success "Bicep template validation passed"

    # Generate deployment name
    $deploymentName = "eshop-deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

    if ($WhatIf) {
        Write-Info "Running what-if analysis..."
        $whatIfArgs = @(
            "deployment", "group", "what-if",
            "--resource-group", $ResourceGroup,
            "--name", $deploymentName,
            "--template-file", "main.bicep",
            "--parameters", "main.bicepparam"
        )
        & az @whatIfArgs
        return
    }

    # Deploy infrastructure
    Write-Info "Starting deployment..."
    $deployArgs = @(
        "deployment", "group", "create",
        "--resource-group", $ResourceGroup,
        "--name", $deploymentName,
        "--template-file", "main.bicep",
        "--parameters", "main.bicepparam"
    )
    
    $deployResult = & az @deployArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Deployment failed"
        return
    }

    Write-Success "Deployment completed successfully!"

    # Get deployment outputs
    Write-Info "Retrieving deployment outputs..."
    $outputs = az deployment group show --resource-group $ResourceGroup --name $deploymentName --query "properties.outputs" | ConvertFrom-Json

    Write-Host ""
    Write-Success "=== Deployment Complete ==="
    
    if ($outputs.webAppUrl) {
        Write-Info "Web Application: $($outputs.webAppUrl.value)"
    }
    if ($outputs.mobileBffUrl) {
        Write-Info "Mobile BFF: $($outputs.mobileBffUrl.value)"
    }
    if ($outputs.serviceEndpoints) {
        Write-Info "Service endpoints available in outputs"
    }

    Write-Host ""
    Write-Info "You can view all service endpoints by running:"
    Write-Info "az deployment group show --resource-group '$ResourceGroup' --name '$deploymentName' --query 'properties.outputs'"

} catch {
    Write-Error "An error occurred: $($_.Exception.Message)"
    exit 1
}