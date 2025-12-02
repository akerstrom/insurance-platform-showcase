using 'main.bicep'

// Deploy to dev environment with Azure Container Registry
param environment = 'dev'
param containerRegistry = 'semodoshowcase.azurecr.io'
param imageTag = 'latest'
