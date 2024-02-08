# Create a Blazor-based frontend web shop

Now that we have an API to provide catalog items, we can start the process of building a frontend web shop that customers can use to browse the catalog. We'll use Blazor to create this web app.

## Review the changes to the eShop solution

The provided starting point for this lab is based on the work you completed in the previous lab, however the Catalog API has been extended with more endpoints and the code refactored for better organization.

1. Open the `eShop.sln` in Visual Studio or VS Code.
1. Note there are some changes to the `Catalog.API` project:
    - The `Apis` directory contains the `CatalogApi.cs` file now, rather than it being in the project root
    - There is an `Extension` directory containing a `HostingExtensions.cs` file, in which an extension method is defined called `AddApplicationServices`. This method is called from the `Program.cs` file to setup the application's services. If any other extension methods are required, they can be placed in classes defined in this directory.
    - There is a `Pics` directory containing product photos for the items in the catalog.
    - There is a `CatalogOptions` class defined in `CatalogOptions.cs` file in the project root. This class is used to support configurable values used in the project.
    - The `Apis/CatalogApi.cs` file now defines many other endpoints for the Catalog API which will be used to support the frontend experience. Take a moment to read the additional endpoint definitions and implementations.
1. The `eShop.ServiceDefaults` project has some changes too, including more extension methods to help with reading required configuration values, and configuring OpenAPI documents from the configuration. Take a moment to read the contents of the new files.
1. Launch the AppHost project and navigate to the Swagger UI for the Catalog API to see the additional endpoints that are now available.

## Add the WebApp project to the AppHost project

The provided starting point for this lab includes a Blazor web project for the frontend of our eShop that we're going to make updates to, but it hasn't yet been composed into the AppHost project.

1. Add a project reference from the `eShop.AppHost` project to the `WebApp` project.
1. Open the `Program.cs` file in the `eShop.AppHost` project and add a line to add the `WebApp` project to the app model, with a reference to the `Catalog.API` project:

    ```csharp
    builder.AddProject<WebApp>("webapp")
        .WithReference(catalogApi);
    ```

1. Run the AppHost project and verify that the `WebApp` project is now included in the list of running resources on the dashboard.
1. Click the first hyperlink for the `WebApp` project in the **Endpoints** column and verify you see the home page of the eShop website.

    ![The starting point for the eShop website](./img/eshop-web-blank-frontpage.png)

## Create a client service for the Catalog API

To communicate with the Catalog API from the web app, we'll create a service class that uses an `HttpClient` instance managed by the [`IHttpClientFactory` features](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory).

1. In the `WebApp` project, create a `Services` directory and inside it create a new file `CatalogService.cs`
1. Declare a class in this file called `CatalogService` with a primary constructor that accepts a single `HttpClient httpClient` parameter, and a field to store the common base path of the Catalog API endpoints `"api/v1/catalog"`:

    ```csharp
    namespace eShop.WebApp.Services;

    public class CatalogService(HttpClient httpClient)
    {
        private readonly string remoteServiceBaseUrl = "api/v1/catalog/";


    }
    ```

1. We need a method that will be called to retrieve a page of catalog items from the API. Add an async method called `GetCatalogItems` that accepts appropriate parameters for the page size, page index, and optional item brand and item type filters, and returns `CatalogResult` (this type is already defined in the project), e.g.:

    ```csharp
    public async Task<CatalogResult> GetCatalogItems(int pageIndex, int pageSize, int? brand, int? type)
    {
        
    }
    ```

1. The Catalog API represents the catalog items in the URL space categorized optionally by brand and type, e.g. `/items`, `/items/type/all`, `/items/type/123/brand/456`, etc. Additionally the paging parameters are passed via the query string. This means the URL called by our service needs to be dynamically constructed based on the values of the parameters passed to this method. Create a new method to handle this job called `GetCatalogItemsUri`, e.g.:

    ```csharp
    private static string GetAllCatalogItemsUri(string baseUri, int pageIndex, int pageSize, int? brand, int? type)
    {
        // Build URLs like:
        //   [base]/items
        //   [base]/items/type/all
        //   [base]/items/type/123/brand/456
        //   [base]/items/type/123/brand/456?pageSize=9&pageIndex=2
        string filterPath;

        if (type.HasValue)
        {
            var brandPath = brand.HasValue ? brand.Value.ToString() : string.Empty;
            filterPath = $"/type/{type.Value}/brand/{brandPath}";

        }
        else if (brand.HasValue)
        {
            var brandPath = brand.HasValue ? brand.Value.ToString() : string.Empty;
            filterPath = $"/type/all/brand/{brandPath}";
        }
        else
        {
            filterPath = string.Empty;
        }

        return $"{baseUri}items{filterPath}?pageIndex={pageIndex}&pageSize={pageSize}";
    }
    ```

1. Using this method, update the body of the `GetCatalogItems` method to retrieve the items from the Catalog API using the `httpClient` constructor parameter:

    ```csharp
    public async Task<CatalogResult> GetCatalogItems(int pageIndex, int pageSize, int? brand, int? type)
    {
        var uri = GetAllCatalogItemsUri(remoteServiceBaseUrl, pageIndex, pageSize, brand, type);
        var result = await httpClient.GetFromJsonAsync<CatalogResult>(uri);
        return result!;
    }
    ```

1. Register the `CatalogService` with the application's DI container and configure its base address to point at the `catalog-api` resource by adding a line to the `Extensions/HostingExtensions.cs` file:

    ```csharp
    // HTTP and gRPC client registrations
    builder.Services.AddHttpClient<CatalogService>(o => o.BaseAddress = new("http://catalog-api"));
    ```

1. Ensure the `WebApp` project builds successfully before continuing.

## Update the home page to display catalog items

Now that we have a service we can use to easily retrieve the catalog items from the Catalog API, let's update the site's home page to use the service and display some catalog items.

1. In the `WebApp` project, open the `Components/Pages/Catalog.razor` file. Note that this page is configured to be the home page (served from the `/` path) via the `@page "/"` directive at the top of the file.
1. In order to get an instance of the `CatalogService` class you 