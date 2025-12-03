# Infrastructure

Azure Bicep templates for deploying Insurance Platform Showcase to Azure Container Apps.

## Prerequisites

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [Docker](https://docs.docker.com/get-docker/)
- Azure subscription with Container Registry (`semodoshowcase.azurecr.io`)

## Build and Push Images

### 1. Login to Azure Container Registry

```bash
az acr login --name semodoshowcase
```

### 2. Build Images

From the repository root:

```bash
docker build -f src/VehicleService/Dockerfile -t semodoshowcase.azurecr.io/ips-vehicle-service:latest .
docker build -f src/InsuranceService/Dockerfile -t semodoshowcase.azurecr.io/ips-insurance-service:latest .
docker build -f src/CustomerService/Dockerfile -t semodoshowcase.azurecr.io/ips-customer-service:latest .
docker build -f legacy/VehicleDatabase/Dockerfile -t semodoshowcase.azurecr.io/ips-legacy-vehicle-db:latest .
docker build -f legacy/InsuranceMainframe/Dockerfile -t semodoshowcase.azurecr.io/ips-legacy-mainframe:latest .
docker build -f client/Dockerfile -t semodoshowcase.azurecr.io/ips-web:latest ./client
```

### 3. Push Images

```bash
docker push semodoshowcase.azurecr.io/ips-vehicle-service:latest
docker push semodoshowcase.azurecr.io/ips-insurance-service:latest
docker push semodoshowcase.azurecr.io/ips-customer-service:latest
docker push semodoshowcase.azurecr.io/ips-legacy-vehicle-db:latest
docker push semodoshowcase.azurecr.io/ips-legacy-mainframe:latest
docker push semodoshowcase.azurecr.io/ips-web:latest
```

## Deployment

### 4. Login to Azure

```bash
az login
az account set --subscription "<your-subscription-id>"
```

### 5. Create Resource Group

```bash
az group create --name ips-rg --location swedencentral
```

### 6. Get ACR Credentials

```bash
# Enable admin user on ACR (if not already enabled)
az acr update --name semodoshowcase --admin-enabled true

# Get credentials
ACR_USERNAME=$(az acr credential show --name semodoshowcase --query username -o tsv)
ACR_PASSWORD=$(az acr credential show --name semodoshowcase --query "passwords[0].value" -o tsv)
```

### 7. Deploy Infrastructure

```bash
az deployment group create \
  --resource-group ips-rg \
  --template-file infra/main.bicep \
  --parameters \
    environment=dev \
    containerRegistry=semodoshowcase.azurecr.io \
    imageTag=latest \
    containerRegistryUsername=$ACR_USERNAME \
    containerRegistryPassword=$ACR_PASSWORD
```

### 8. Get Application URLs

```bash
# Get the web UI URL (main entry point)
az deployment group show \
  --resource-group ips-rg \
  --name main \
  --query properties.outputs.webUrl.value \
  --output tsv

# Get the customer service API URL (if needed for direct access)
az deployment group show \
  --resource-group ips-rg \
  --name main \
  --query properties.outputs.customerServiceUrl.value \
  --output tsv
```

## Deploy New Version

After making code changes, rebuild and push the updated images, then update the container apps.

### 1. Rebuild and Push Images

```bash
# Login to ACR (if session expired)
az acr login --name semodoshowcase

# Rebuild changed services (from repository root)
docker build -f src/CustomerService/Dockerfile -t semodoshowcase.azurecr.io/ips-customer-service:latest .

# Push updated images
docker push semodoshowcase.azurecr.io/ips-customer-service:latest
```

### 2. Update Container Apps

```bash
# Update specific service (creates new revision with latest image)
az containerapp update -n ips-customer-service -g ips-rg \
  --image semodoshowcase.azurecr.io/ips-customer-service:latest

# Or update all services
az containerapp update -n ips-web -g ips-rg --image semodoshowcase.azurecr.io/ips-web:latest
az containerapp update -n ips-customer-service -g ips-rg --image semodoshowcase.azurecr.io/ips-customer-service:latest
az containerapp update -n ips-vehicle-service -g ips-rg --image semodoshowcase.azurecr.io/ips-vehicle-service:latest
az containerapp update -n ips-insurance-service -g ips-rg --image semodoshowcase.azurecr.io/ips-insurance-service:latest
az containerapp update -n ips-legacy-vehicle-db -g ips-rg --image semodoshowcase.azurecr.io/ips-legacy-vehicle-db:latest
az containerapp update -n ips-legacy-mainframe -g ips-rg --image semodoshowcase.azurecr.io/ips-legacy-mainframe:latest
```

### 3. Verify Deployment

```bash
# Check revision status
az containerapp revision list -n ips-customer-service -g ips-rg -o table

# View logs
az containerapp logs show -n ips-customer-service -g ips-rg --follow
```

## Architecture

```
Internet (Browser)
    │
    ├──────────────────────────────────┐
    │                                  │
    ▼                                  ▼
┌─────────────────────────────────────────────────┐
│         Azure Container Apps Environment        │
│                                                 │
│  ┌─────┐                                        │
│  │ web │ ◄── external (HTTPS)                   │
│  └─────┘     static files + /config.json        │
│                                                 │
│  ┌──────────────────┐                           │
│  │ customer-service │ ◄── external (HTTPS)      │
│  └────────┬─────────┘     direct CORS calls     │
│           │                from browser         │
│     ┌─────┴─────┐                               │
│     ▼           ▼                               │
│  ┌─────────┐ ┌─────────┐                        │
│  │insurance│ │ vehicle │  ◄── internal only     │
│  │-service │ │-service │                        │
│  └────┬────┘ └────┬────┘                        │
│       │           │                             │
│       ▼           ▼                             │
│  ┌─────────┐ ┌─────────┐                        │
│  │ legacy- │ │ legacy- │  ◄── internal only     │
│  │mainframe│ │vehicle- │                        │
│  │         │ │   db    │                        │
│  └─────────┘ └─────────┘                        │
└─────────────────────────────────────────────────┘
```

**Key points:**
- The browser loads static files from `ips-web` and fetches `/config.json` for API configuration
- The browser calls `ips-customer-service` directly via CORS (no nginx proxy)
- The web container injects `API_BASE_URL` at startup using `envsubst`

## Resource Sizing (Free Tier Optimized)

| Service | CPU | Memory | Min Replicas | Max Replicas |
|---------|-----|--------|--------------|--------------|
| web | 0.25 | 0.5Gi | 0 | 1 |
| customer-service | 0.25 | 0.5Gi | 0 | 1 |
| insurance-service | 0.25 | 0.5Gi | 0 | 1 |
| vehicle-service | 0.25 | 0.5Gi | 0 | 1 |
| legacy-mainframe | 0.25 | 0.5Gi | 0 | 1 |
| legacy-vehicle-db | 0.25 | 0.5Gi | 0 | 1 |

All services scale to zero when idle, minimizing costs.

## Health Probes

All container apps are configured with health probes to handle cold start scenarios:

| Probe Type | Path | Initial Delay | Period | Failure Threshold |
|------------|------|---------------|--------|-------------------|
| Startup | `/health` | 5s | 3s | 10 |
| Readiness | `/health` | - | 5s | 3 |

The startup probe allows up to 35 seconds for cold start (5s initial + 10 failures * 3s period).

## Configuration

### HttpClient Timeout

Services use a configurable HttpClient timeout (default: 30s) to handle cold start latency:

```json
{
  "HttpClient": {
    "TimeoutSeconds": 30
  }
}
```

This can be overridden via environment variables: `HttpClient__TimeoutSeconds=60`

### Web Container Runtime Config

The web container uses runtime configuration injection to locate the Customer Service API:

1. **Template file**: `client/config.template.json` contains `${API_BASE_URL}` placeholder
2. **Startup**: Container runs `envsubst` to generate `/usr/share/nginx/html/config.json`
3. **Runtime**: Frontend fetches `/config.json` to get the API URL

The `API_BASE_URL` environment variable is set by the Bicep template to point to the Customer Service FQDN:

```
API_BASE_URL=https://ips-customer-service.<env-domain>.azurecontainerapps.io
```

This pattern allows the same Docker image to work across different environments without rebuilding.

## Clean Up

```bash
az group delete --name ips-rg --yes --no-wait
```
