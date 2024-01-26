var builder = DistributedApplication.CreateBuilder(args);

// Databases

var basketStore = builder.AddRedis("BasketStore").WithRedisCommander();
var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var catalogDb = postgres.AddDatabase("CatalogDB");
var orderDb = postgres.AddDatabase("OrderingDB");

// Identity Providers

var keycloak = builder.AddKeycloakContainer("idp", tag: "23.0")
    .ImportRealms("../Keycloak/data/import");

// API Apps

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb);

var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
    .WithReference(basketStore)
    .WithReference(keycloak);

var orderingApi = builder.AddProject<Projects.Ordering_API>("ordering-api")
    .WithReference(orderDb)
    .WithReference(keycloak);

// Apps

var webApp = builder.AddProject<Projects.WebApp>("webapp")
    .WithReference(basketApi)
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(keycloak)
    // Force HTTPS profile for web app (required for OIDC operations)
    .WithLaunchProfile("https");

// Inject the project URLs for Keycloak realm configuration
keycloak.WithEnvironment("WEBAPP_HTTP", () => webApp.GetEndpoint("http").UriString);
keycloak.WithEnvironment("WEBAPP_HTTPS", () => webApp.GetEndpoint("https").UriString);
keycloak.WithEnvironment("ORDERINGAPI_HTTP", () => orderingApi.GetEndpoint("http").UriString);

builder.Build().Run();
