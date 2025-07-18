# Infrastructure

This directory contains the Infrastructure as Code (IaC) for deploying the DeepResearch application to Azure using Azure Developer CLI (azd).

## Prerequisites

1. **Azure Developer CLI (azd)**
   - Install from: https://aka.ms/azure-dev/install

2. **Azure CLI**
   - Install from: https://docs.microsoft.com/cli/azure/install-azure-cli

3. **Tavily API Key**
   - Get an API key from [Tavily](https://tavily.com/)
   - This is required for the web search functionality

## Deployment

1. Login to Azure:
   ```bash
   azd auth login
   ```

2. Initialize the environment (first time only):
   ```bash
   azd init
   ```

3. Set the Tavily API key:
   ```bash
   azd env set TAVILY_API_KEY "your-tavily-api-key-here"
   ```

4. Deploy the infrastructure and application:
   ```bash
   azd up
   ```

## Resources Created

The infrastructure template creates the following Azure resources:

- **Resource Group**: Contains all resources for the environment
- **App Service Plan**: Hosts the web application (B1 SKU)
- **App Service**: Runs the .NET web application
- **Azure OpenAI**: Provides GPT-4o-mini model for AI functionality
- **Managed Identity**: Allows the web app to access Azure OpenAI securely

## Configuration

The deployment uses the following environment variables:

- `AZURE_ENV_NAME`: Name of the environment
- `AZURE_LOCATION`: Azure region for deployment
- `AZURE_PRINCIPAL_ID`: Principal ID for role assignments
- `TAVILY_API_KEY`: API key for Tavily search service

## Security

- The web application uses Managed Identity to authenticate with Azure OpenAI
- The Tavily API key is stored as an application setting (secure parameter)
- HTTPS is enforced on the App Service