# Add shopping basket capabilities to the web site

In previous labs, we have created a web site that shoppers can use to browser a through pages of products, optionally with filtering by brand or type, and added the ability for users to create an account and sign in. In this lab, we will add the capability to add products to a shopping basket. The shopping basket will be stored in Redis, and exposed via a new gRPC service that the web site will communicate with.

1. Add a new project called `Basket.API` to the solution using the **ASP.NET Core gRPC Service** template. Ensure that the following template options are configured:

    - Framework: **.NET 8.0 (Long Term Support)**
    - Enable Docker: **disabled**
    - Do not use top-level statements: **disabled**
    - Enable native AOT publish: **disabled**
    - Elinst in .NET Aspire orchestration: **enabled**

    ![VS gRPC Service project template options](./img/vs-grpc-template-options.png)

1. Open the `Program.cs` file in the `eShop.AppHost` project and add a line to create a new Redis resource named `"BasketStore"` and configure it to host a [Redis Commander](https://joeferner.github.io/redis-commander/) instance too (this will make it easier to inspect the Redis database during development). Capture the resource in a `basketStore` variable:

    ```csharp
    var basketStore = builder.AddRedis("BasketStore").WithRedisCommander();
    ```

    Aspire will automatically create containers for both Redis and Redis Commander  when the application is run.

1. Find the line that was added to add the `Basket.API` gRPC project to the AppHost as a resource. Update the code to name the resource `"basket-api"`, make it reference the `idp` and `BasketStore` resources, and capture it in a `basketApi` variable:

    ```csharp
    var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
        .WithReference(idp)
        .WithReference(basketStore);
    ```

    The `Basket.API` will require calls to be authenticated by the IdP, and will need to access the Redis database to store and retrieve shopping baskets.

1. Update the `webapp` resource to reference the `basket-api` resource so the web site can communicate with the Basket API:

    ```csharp
    var webApp = builder.AddProject<Projects.WebApp>("webapp")
        .WithReference(catalogApi)
        .WithReference(basketApi)
        .WithReference(idp)
        // Force HTTPS profile for web app (required for OIDC operations)
        .WithLaunchProfile("https");
    ```

1. Run the AppHost project and verify that the containers for Redis and Redis Commander are created and running by using the dashboard. Also verify that the `Basket.API` project is running and that it's environment variables contain the configuration values to communicate with the IdP and Redis.