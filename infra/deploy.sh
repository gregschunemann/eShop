#!/bin/bash

# ==================================================================================================
# eShop Azure Deployment Script
# ==================================================================================================
# This script deploys the eShop microservices application to Azure using Bicep templates.
# It provides options for different environments and deployment configurations.

set -e

# Default values
RESOURCE_GROUP=""
ENVIRONMENT="dev"
LOCATION="East US"
NAME_PREFIX="eshop"
SUBSCRIPTION_ID=""
ENABLE_OPENAI="false"
ENABLE_ZONE_REDUNDANCY="false"
CONTAINER_REGISTRY=""
IMAGE_TAG="latest"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
print_usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -g, --resource-group    Resource group name (required)"
    echo "  -e, --environment       Environment name (dev, staging, prod) [default: dev]"
    echo "  -l, --location          Azure region [default: East US]"
    echo "  -p, --prefix            Name prefix for resources [default: eshop]"
    echo "  -s, --subscription      Azure subscription ID"
    echo "  --enable-openai         Enable OpenAI integration [default: false]"
    echo "  --enable-zone-redundancy Enable zone redundancy [default: false]"
    echo "  --acr-server            Existing container registry server"
    echo "  --image-tag             Container image tag [default: latest]"
    echo "  -h, --help              Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 -g rg-eshop-dev -e dev"
    echo "  $0 -g rg-eshop-prod -e prod --enable-zone-redundancy --enable-openai"
    echo "  $0 -g rg-eshop-staging -e staging --acr-server myregistry.azurecr.io"
}

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -g|--resource-group)
            RESOURCE_GROUP="$2"
            shift 2
            ;;
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -l|--location)
            LOCATION="$2"
            shift 2
            ;;
        -p|--prefix)
            NAME_PREFIX="$2"
            shift 2
            ;;
        -s|--subscription)
            SUBSCRIPTION_ID="$2"
            shift 2
            ;;
        --enable-openai)
            ENABLE_OPENAI="true"
            shift
            ;;
        --enable-zone-redundancy)
            ENABLE_ZONE_REDUNDANCY="true"
            shift
            ;;
        --acr-server)
            CONTAINER_REGISTRY="$2"
            shift 2
            ;;
        --image-tag)
            IMAGE_TAG="$2"
            shift 2
            ;;
        -h|--help)
            print_usage
            exit 0
            ;;
        *)
            log_error "Unknown option: $1"
            print_usage
            exit 1
            ;;
    esac
done

# Validate required parameters
if [[ -z "$RESOURCE_GROUP" ]]; then
    log_error "Resource group is required"
    print_usage
    exit 1
fi

# Set subscription if provided
if [[ -n "$SUBSCRIPTION_ID" ]]; then
    log_info "Setting Azure subscription to $SUBSCRIPTION_ID"
    az account set --subscription "$SUBSCRIPTION_ID"
fi

# Verify Azure CLI login
log_info "Verifying Azure CLI login..."
if ! az account show > /dev/null 2>&1; then
    log_error "Not logged in to Azure CLI. Please run 'az login' first."
    exit 1
fi

CURRENT_SUBSCRIPTION=$(az account show --query id -o tsv)
log_info "Using Azure subscription: $CURRENT_SUBSCRIPTION"

# Create resource group if it doesn't exist
log_info "Checking if resource group '$RESOURCE_GROUP' exists..."
if ! az group show --name "$RESOURCE_GROUP" > /dev/null 2>&1; then
    log_info "Creating resource group '$RESOURCE_GROUP' in '$LOCATION'..."
    az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
    log_success "Resource group created successfully"
else
    log_info "Resource group '$RESOURCE_GROUP' already exists"
fi

# Build deployment parameters
DEPLOYMENT_PARAMS="environmentName=$ENVIRONMENT"
DEPLOYMENT_PARAMS="$DEPLOYMENT_PARAMS namePrefix=$NAME_PREFIX"
DEPLOYMENT_PARAMS="$DEPLOYMENT_PARAMS imageTag=$IMAGE_TAG"
DEPLOYMENT_PARAMS="$DEPLOYMENT_PARAMS enableZoneRedundancy=$ENABLE_ZONE_REDUNDANCY"

if [[ -n "$CONTAINER_REGISTRY" ]]; then
    DEPLOYMENT_PARAMS="$DEPLOYMENT_PARAMS acrLoginServer=$CONTAINER_REGISTRY"
fi

# Handle OpenAI parameters
if [[ "$ENABLE_OPENAI" == "true" ]]; then
    DEPLOYMENT_PARAMS="$DEPLOYMENT_PARAMS enableOpenAI=true"
    
    # Prompt for OpenAI credentials if not provided
    if [[ -z "$OPENAI_ENDPOINT" ]]; then
        read -p "Enter OpenAI endpoint URL: " OPENAI_ENDPOINT
    fi
    if [[ -z "$OPENAI_API_KEY" ]]; then
        read -s -p "Enter OpenAI API key: " OPENAI_API_KEY
        echo
    fi
    
    DEPLOYMENT_PARAMS="$DEPLOYMENT_PARAMS openAIEndpoint=$OPENAI_ENDPOINT"
    DEPLOYMENT_PARAMS="$DEPLOYMENT_PARAMS openAIApiKey=$OPENAI_API_KEY"
fi

# Show deployment summary
echo ""
log_info "=== Deployment Summary ==="
log_info "Resource Group: $RESOURCE_GROUP"
log_info "Environment: $ENVIRONMENT"
log_info "Location: $LOCATION"
log_info "Name Prefix: $NAME_PREFIX"
log_info "Image Tag: $IMAGE_TAG"
log_info "Zone Redundancy: $ENABLE_ZONE_REDUNDANCY"
log_info "OpenAI Integration: $ENABLE_OPENAI"
if [[ -n "$CONTAINER_REGISTRY" ]]; then
    log_info "Container Registry: $CONTAINER_REGISTRY"
fi
echo ""

# Confirm deployment
read -p "Proceed with deployment? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    log_info "Deployment cancelled"
    exit 0
fi

# Validate Bicep template
log_info "Validating Bicep template..."
if ! az deployment group validate \
    --resource-group "$RESOURCE_GROUP" \
    --template-file main.bicep \
    --parameters $DEPLOYMENT_PARAMS > /dev/null; then
    log_error "Bicep template validation failed"
    exit 1
fi
log_success "Bicep template validation passed"

# Deploy infrastructure
log_info "Starting deployment..."
DEPLOYMENT_NAME="eshop-deployment-$(date +%Y%m%d-%H%M%S)"

if az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --template-file main.bicep \
    --parameters $DEPLOYMENT_PARAMS; then
    
    log_success "Deployment completed successfully!"
    
    # Get deployment outputs
    log_info "Retrieving deployment outputs..."
    WEB_APP_URL=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$DEPLOYMENT_NAME" --query "properties.outputs.webAppUrl.value" -o tsv)
    MOBILE_BFF_URL=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$DEPLOYMENT_NAME" --query "properties.outputs.mobileBffUrl.value" -o tsv)
    
    echo ""
    log_success "=== Deployment Complete ==="
    log_info "Web Application: $WEB_APP_URL"
    log_info "Mobile BFF: $MOBILE_BFF_URL"
    echo ""
    log_info "You can view all service endpoints in the Azure portal or by running:"
    log_info "az deployment group show --resource-group '$RESOURCE_GROUP' --name '$DEPLOYMENT_NAME' --query 'properties.outputs'"
    
else
    log_error "Deployment failed"
    exit 1
fi