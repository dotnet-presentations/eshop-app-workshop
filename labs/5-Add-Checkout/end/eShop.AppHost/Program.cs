using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Databases

var basketStore = builder.AddRedis("BasketStore").WithRedisCommander();
var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var catalogDb = postgres.AddDatabase("CatalogDB");
var orderDb = postgres.AddDatabase("OrderingDB");

// Identity Providers

var idp = builder.AddKeycloakContainer("idp", tag: "23.0")
    .ImportRealms("../Keycloak/data/import");

// DB Manager Apps

builder.AddProject<Catalog_Data_Manager>("catalog-db-mgr")
    .WithReference(catalogDb);

builder.AddProject<Ordering_Data_Manager>("ordering-db-mgr")
    .WithReference(orderDb);

// API Apps

var catalogApi = builder.AddProject<Catalog_API>("catalog-api")
    .WithReference(catalogDb);

var basketApi = builder.AddProject<Basket_API>("basket-api")
    .WithReference(basketStore)
    .WithReference(idp);

// Apps

var webApp = builder.AddProject<WebApp>("webapp")
    .WithReference(basketApi)
    .WithReference(catalogApi)
    .WithReference(idp)
    // Force HTTPS profile for web app (required for OIDC operations)
    .WithLaunchProfile("https");

// Inject the project URLs for Keycloak realm configuration
idp.WithEnvironment("WEBAPP_HTTP", () => webApp.GetEndpoint("http").UriString);
idp.WithEnvironment("WEBAPP_HTTPS", () => webApp.GetEndpoint("https").UriString);
idp.WithEnvironment("ORDERINGAPI_HTTP", () => "http://placeholder-for-ordering-api");

// Inject assigned URLs for Catalog API
catalogApi.WithEnvironment("CatalogOptions__PicBaseAddress", () => catalogApi.GetEndpoint("http").UriString);

builder.Build().Run();
