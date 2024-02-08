using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Databases

var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var catalogDb = postgres.AddDatabase("CatalogDB");

// Identity Providers

var idp = builder.AddKeycloakContainer("idp", tag: "23.0")
    .ImportRealms("../Keycloak/data/import");

// DB Manager Apps

builder.AddProject<Catalog_Data_Manager>("catalog-db-mgr")
    .WithReference(catalogDb);


// API Apps

var catalogApi = builder.AddProject<Catalog_API>("catalog-api")
    .WithReference(catalogDb);

// Apps


var webApp = builder.AddProject<WebApp>("webapp")
    .WithReference(catalogApi);

// Inject the project URLs for Keycloak realm configuration
idp.WithEnvironment("WEBAPP_HTTP", () => webApp.GetEndpoint("http").UriString);
idp.WithEnvironment("WEBAPP_HTTPS", () => webApp.GetEndpoint("https").UriString);

// Inject assigned URLs for Catalog API
catalogApi.WithEnvironment("CatalogOptions__PicBaseAddress", () => catalogApi.GetEndpoint("http").UriString);

builder.Build().Run();
