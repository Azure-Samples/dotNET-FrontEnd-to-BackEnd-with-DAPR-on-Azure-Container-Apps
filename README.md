# ASP.NET Core Front-end + 2 Back-end APIs on Azure Container Apps

This repository contains a simple scenario built to demonstrate how ASP.NET Core 6.0 can be used to build a cloud-native application hosted in Azure Container Apps. The repository consists of the following projects and folders:

* Store - A Blazor server project representing the frontend of an online store. The store's UI shows a list of all the products in the store, and their associated inventory status. 
* Products API - A simple API that generates fake product names using the open-source NuGet package [Bogus](https://github.com/bchavez/Bogus). 
* Inventory API - A simple API that provides a random number for a given product ID string. The values of each string/integer pair are stored in memory cache so they are consistent between API calls. 
* Infra folder - contains Azure Bicep files used for creating and configuring all the Azure resources. 
* GitHub Actions workflow file used to deploy the app using CI/CD. 

## What you'll learn

This exercise will introduce you to a variety of concepts, with links to supporting documentation throughout the tutorial. 

* [Azure Container Apps](https://docs.microsoft.com/azure/container-apps/overview)
* [GitHub Actions](https://github.com/features/actions)
* [Azure Container Registry](https://docs.microsoft.com/azure/container-registry/)
* [Azure Bicep](https://docs.microsoft.com/azure/azure-resource-manager/bicep/overview?tabs=**bicep**)

## Prerequisites

You'll need an Azure subscription and a very small set of tools and skills to get started:

1. An Azure subscription. Sign up [for free](https://azure.microsoft.com/free/).
2. A GitHub account, with access to GitHub Actions.
3. Either the [Azure Developer CLI](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd?tabs=winget-windows%2Cbrew-mac%2Cscript-linux&pivots=os-windows) installed locally, or, access to [GitHub Codespaces](https://github.com/features/codespaces), which would enable you to do develop in your browser.
4. Docker Desktop installed locally, or, access to a [Docker Desktop Codespace](https://docs.github.com/en/codespaces/getting-started/deep-dive)

## Topology diagram

The resultant application is an Azure Container Environment-hosted set of containers - the `products` API, the `inventory` API, and the `store` Blazor Server front-end.

![Application topology](docs/media/topology.png)

Internet traffic should not be able to directly access either of the back-end APIs as each of these containers is marked as "internal ingress only" during the deployment phase. Internet traffic hitting the `<aca name>.<your app>.<your region>.azurecontainerapps.io` URL should be proxied to the `frontend` container, which in turn makes outbound calls to both the `products` and `inventory` APIs within the Azure Container Apps Environment.

## Setup

By the end of this section you'll have a 3-node app running in Azure. This setup process consists of two steps, and should take you around 15 minutes. 

1. Fork this repository to your own GitHub organization.

## Deploy the code using `AZD` CLI

The easiest way to deploy the code is to use the `AZD` CLI. This CLI is a wrapper around the Azure CLI that simplifies the process of deploying Azure resources. Run the following commands:

```pwsh
azd auth login --tenant-id <tenantname>.onmicrosoft.com
```

## Deploy the code using GitHub Actions

The `AZD` CLI is available as a GitHub Action, so you can use it in your CI/CD workflows.

This repo contains a GitHub Actions workflow file that deploys the code to Azure Container Apps. The workflow file is located at `.github/workflows/azure-dev.yml`.

Run the following command to configure the GitHub Actions workflow:

```pwsh
azd pipeline config
```
The workflow file is triggered by a push to the `main` branch.

With the projects deployed to Azure, you can now test the app to make sure it works. 

## Try the app in Azure

The `deploy` CI/CD process creates a series of resources in your Azure subscription. These are used primarily for hosting the project code, but there's also a few additional resources that aid with monitoring and observing how the app is running in the deployed environment. 
| Resource  | Resource Type                                                | Purpose                                                      |
| --------- | ------------------------------------------------------------ | ------------------------------------------------------------ |
| storeai   | Application Insights                                         | This provides telemetry and diagnostic information for when I want to monitor how the app is performing or for when things need troubleshooting. |
| store     | An Azure Container App that houses the code for the front end. | The store app is the store's frontend app, running a Blazor Server project that reaches out to the backend APIs |
| products  | An Azure Container App that houses the code for a minimal API. | This API is a Swagger UI-enabled API that hands back product names and IDs to callers. |
| inventory | An Azure Container App that houses the code for a minimal API. | This API is a Swagger UI-enabled API that hands back quantities for product IDs. A client would need to call the `products` API first to get the product ID list, then use those product IDs as parameters to the `inventory` API to get the quantity of any particular item in inventory. |
| storeenv  | An Azure Container Apps Environment                          | This environment serves as the networking meta-container for all of the instances of all of the container apps comprising the app. |
| storeacr  | An Azure Container Registry                                  | This is the container registry into which the CI/CD process publishes my application containers when I commit code to the `deploy` branch. From this registry, the containers are pulled and loaded into Azure Container Apps. |
| storelogs | Log Analytics Workspace                                      | This is where I can perform custom [Kusto](https://docs.microsoft.com/azure/data-explorer/kusto/query/) queries against the application telemetry, and time-sliced views of how the app is performing and scaling over time in the environment. |

The resources are shown here in the Azure portal:

![Resources in the portal](docs/media/azure-portal.png)

Click on the `store` container app to open it up in the Azure portal. In the `Overview` tab you'll see a URL. 

![View the store's public URL.](docs/media/get-public-url.png)

Clicking that URL will open the app's frontend up in the browser. 

![The product list, once the app is running.](docs/media/store-ui.png)

You'll see that the first request will take slightly longer than subsequent requests. On the first request to the page, the APIs are called on the server side. The code uses `IMemoryCache` to store the results of the API calls in memory. So, subsequent calls will use the cached payload rather than make live requests each time. 
