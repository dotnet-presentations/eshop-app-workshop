# Create a Blazor-based frontend web shop

Now that we have an API to provide catalog items, we can start the process of building a frontend web shop that customers can use to browse the catalog. We'll use Blazor to create this web app.

## Review the changes to the Catalog API

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