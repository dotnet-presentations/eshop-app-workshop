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

var orderingApi = builder.AddProject<Ordering_API>("ordering-api")
    .WithReference(orderDb)
    .WithReference(idp);

// Apps

// Force HTTPS profile for web app (required for OIDC operations)
var webApp = builder.AddProject<WebApp>("webapp", launchProfileName: "https")
    .WithReference(basketApi)
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(idp);

// Inject the project URLs for Keycloak realm configuration
idp.WithEnvironment("WEBAPP_HTTP", webApp.GetEndpoint("http"));
idp.WithEnvironment("WEBAPP_HTTPS", webApp.GetEndpoint("https"));
idp.WithEnvironment("ORDERINGAPI_HTTP", orderingApi.GetEndpoint("http"));

// Inject assigned URLs for Catalog API
catalogApi.WithEnvironment("CatalogOptions__PicBaseAddress", catalogApi.GetEndpoint("http"));

builder.Build().Run();
