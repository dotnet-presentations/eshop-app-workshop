# Add checkout functionality to the web site

Similar to [lab 1](../1-Create-Catalog-API/README.md), a database has already been defined to store order details for eShop, along with an Entity Framework Core model, and a web app that ensures the database is created and updated to the latest schema by running migrations on startup.

![ERD for the Ordering database](./img/ordering-db-erd.png)

## Getting familiar with the Ordering Database

1. Open the [`eShop.sln`](./src/eShop.sln) in Visual Studio or VS Code.
1. An Entity Framework Core model is already defined for this database in the `Ordering.Data` project. Open the `OrderingDbContext.cs` file in this project and look at the code to see that the the various tables are defined via properties and [classes implementing `IEntityTypeConfiguration<TEntity>`](https://learn.microsoft.com/ef/core/modeling/#grouping-configuration).
1. The `Ordering.Data` project only defines the `DbContext` and entity types. The [EF Core migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/) are defined and managed in the `Ordering.Data.Manager` project. This is a web project that includes some custom code to facilitate creating and seeding the database when the application starts.
1. The AppHost has already been configured to create a PostgreSQL container resource named `OrderingDB` and had the `Ordering.Data.Manager` project added to it as a resource named `ordering-db-mgr` with a reference to the `OrderingDB` database.
1. Run the AppHost project and verify using the dashboard and the **pgAdmin** tool that the `OrderingDB` database has been created and contains the tables defined by the Entity Framework Core migrations.

    ![pgAdmin UI showing the created tables](./img/pgadmin-ordering-db-tables.png)

## Create the Ordering API project

Now that we've verified the Ordering database is working, let's add an HTTP API that provides the ordering capabilities to our system.

1. Add a new project to the solution using the **ASP.NET Core Web API** project template and call it `Ordering.API`, and ensure the following options are configured
    - Framework: **.NET 8.0 (Long Term Support)**
    - Configure for HTTPS: **disabled**
    - Enable Docker: **disabled**
    - Enable OpenAPI support: **enabled**
    - Do not use top-level statements: **disabled**
    - Use controllers: **disabled**
    - Enlist in .NET Aspire orchestration: **enabled**

    ![VS Web API project template options](./img/vs-web-api-template-options.png)

1. In the newly created project, update the package reference to `Swashbuckle.AspNetCore` to version `6.5.0`
1. Open the `Program.cs` file of the `eShop.AppHost` project, and update it so the API project you just added is named `"ordering-api"` and has a reference to the `OrderingDB` and the IdP:

    ```csharp
    var orderingApi = builder.AddProject<Projects.Ordering_API>("ordering-api")
        .WithReference(orderDb)
        .WithReference(idp);
    ```

1. Towards the bottom of the `Program.cs` file, udpate the line that adds the `"ORDERINGAPI_HTTP"` envionment variable to the `idp` resource so that it now passes in the `http` endpoint from the `orderingApi` resource. This will ensure the IdP is configured correctly to support authentication requests from the `Ordering.API` project:

    ```csharp
    idp.WithEnvironment("ORDERINGAPI_HTTP", () => orderingApi.GetEndpoint("http").UriString);
    ```

1. Add a project reference from the `Ordering.API` project to the `Ordering.Data` project so that it can use Entity Framework Core to access the database.
1. Open the `Program.cs` file of the `Ordering.API` project and delete the sample code that defines the weather forecasts API.
1. Immediately after the line that calls `builder.AddServiceDefaults()`, add lines to register the default OpenAPI services, and the default authentication services. Reminder, these methods are defined in the `eShop.ServiceDefaults` project and make it easy to add common services to an API project and ensure they're configured consistently:

    ```csharp
    builder.AddServiceDefaults();
    builder.AddDefaultOpenApi();
    builder.AddDefaultAuthentication();
    ```

1. Add a line to configure the `OrderingDbContext` in the application's DI container using the [**Npgsql Entity Framework Core Provider**](https://www.npgsql.org/efcore/index.html) for PostgreSQL. Ensure that the name passed to the method matches the name defined for the database in the AppHost project's `Program.cs` file (`"OrderingDB"`). The `AddNpgsqlDbContext` method comes from the [`Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` Aspire component](https://learn.microsoft.com/dotnet/aspire/database/postgresql-entity-framework-component):

    ```csharp
    builder.AddNpgsqlDbContext<OrderingDbContext>("OrderingDB");
    ```

1. Create a new file called `OrderingApi.cs` and define a static class inside of it called `OrderingApi` in the `Microsoft.AspNetCore.Builder` namespace:

    ```csharp
    namespace Microsoft.AspNetCore.Builder;

    public static class OrderingApi
    {
        
    }
    ```

1. In this class, add an extension method named `MapOrdersApi` on the `RouteGroupBuilder` type, returning that same type:

    ```csharp
    public static RouteGroupBuilder MapOrdersApi(this RouteGroupBuilder app)
    {
        
        return app;
    }
    ```

    This method will define the endpoint routes for the Ordering API.

1. Create a `Models` directory and inside it create a new file `OrderSummary.cs`. In this, define a class called `OrderSummary` with properties to represent the order number, date, status, and total price, and a static method to create an instance of this class from an `Order` entity from the `Ordering.Data` project:

    ```csharp
    using eShop.Ordering.Data;

    namespace eShop.Ordering.API.Model;

    public class OrderSummary
    {
        public int OrderNumber { get; init; }

        public DateTime Date { get; init; }

        public required string Status { get; init; }

        public decimal Total { get; init; }

        public static OrderSummary FromOrder(Order order)
        {
            return new OrderSummary
            {
                OrderNumber = order.Id,
                Date = order.OrderDate,
                Status = order.OrderStatus.ToString(),
                Total = order.GetTotal()
            };
        }
    }
    ```

    This class will be used to represent a summary of an order in the API responses.

1. Back in the `OrderingApi.cs` file, in the `MapOrdersApi` method, add a call to `app.MapGet` to define an endpoint that responds to GET requests to the `/` path, and is handled by an async lambda that accepts two parameters: a `ClaimsPrincipal` type that will be auto-populated with the current user, and the `OrderingDbContext` instance that will come from the DI container:

    ```csharp
    app.MapGet("/", async (ClaimsPrincipal user, OrderingDbContext dbContext) =>
    {
        
    });
    ```

1. Add code to the lambda body to extract the user ID from the `ClaimsPrincipal` and use it to query the database for the orders that belong to that user. If the user ID is null, throw an exception with a relevant message, otherwise return an instance of `OrderSummary` representing the user's orders:

    ```csharp
    app.MapGet("/", async (ClaimsPrincipal user, OrderingDbContext dbContext) =>
    {
        var userId = user.GetUserId()
            ?? throw new InvalidOperationException("User identity could not be found. This endpoint requires authorization.");

        var orders = await dbContext.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Buyer)
            .Where(o => o.Buyer.IdentityGuid == userId)
            .Select(o => OrderSummary.FromOrder(o))
            .AsNoTracking()
            .ToArrayAsync();

        return TypedResults.Ok(orders);
    });
    ``` 

1. In `Program.cs`, immediately before the call to `app.Run()` at the end of the file, add a line to map a route group for the path `/api/v1/orders` and call the `MapOrdersApi` method on it, followed by a call to `RequireAuthorization` to ensure that the endpoint requires a user to be authenticated to access it. This will ensure that the endpoint is only accessible to authenticated users, and that the user's identity is available in the `ClaimsPrincipal` parameter of the lambda in the `MapGet` method:

    ```csharp
    app.MapGroup("/api/v1/orders")
       .MapOrdersApi()
       .RequireAuthorization();
    ```

1. Open the `appsettings.json` file and add the following configuration sections:

    ```json
    {
        // Add the following sections
        "OpenApi": {
            "Endpoint": {
                "Name": "Ordering.API v1"
            },
            "Document": {
                "Description": "The Ordering Service HTTP API",
                "Title": "eShop - Ordering HTTP API",
                "Version": "v1"
            },
            "Auth": {
                "ClientId": "orderingswaggerui",
                "AppName": "Ordering Swagger UI"
            }
        },
        "Identity": {
            "Audience": "orders"
        }
    }
    ```

    The `"OpenApi"` section will ensure the call to `builder.AddDefaultOpenApi()` in the `Program.cs` file configures the Swagger UI for authentication against our IdP. The `"Identity"` section will ensure the call to `builder.AddDefaultAuthentication()` in the `Program.cs` file configures the API for authentication bia JWT Bearer (similar to what we did for the Basket API).

1. Run the AppHost project and verify that the `Ordering.API` project is running and that the `/api/v1/orders` endpoint is visible in the Swagger UI. There should be an **Authorize** button displayed with an open padlock, indicating that the endpoint requires authentication.

    ![Swagger UI showing the endpoint and 'Authorize' button](./img/ordering-api-swagger-unauthenticated.png)

1. Click the **Authorize** button and in the **Available authorizations** dialog opened, click the **Authorize** button to be taken to the sign-in form for the IdP. Sign in with the same test user you used to sign in to the web site in the previous lab (*test@example.com / P@$$w0rd1*). You should be redirected back to the Swagger UI and see the **Available authorizations** dialog again indicating you are now authorized.

    ![Swagger UI showing the 'Available authorizations' dialog in authorized state](./img/ordering-api-swagger-authorizations-dialog.png)

1. Click the **Close** button to close the **Available authorizations** dialog and then click the **Try it out** button for the `/api/v1/orders` endpoint. You should see a response with an empty array of orders, indicating that the endpoint is working and returning the expected response.

    ![Swagger UI showing the endpoint and 'Authorize' button](./img/ordering-api-swagger-try-it-out-response.png)