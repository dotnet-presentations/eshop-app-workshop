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
        .WithReference(catalogApi)
    .WithReference(idp, env: "Identity__ClientSecret");

// Inject the project URLs for Keycloak realm configuration
var webAppHttp = webApp.GetEndpoint("http");
var webAppHttps = webApp.GetEndpoint("https");
idp.WithEnvironment("WEBAPP_HTTP_CONTAINERHOST", webAppHttp);
idp.WithEnvironment("WEBAPP_HTTP", () => $"{webAppHttp.Scheme}://{webAppHttp.Host}:{webAppHttp.Port}");
if (webAppHttps.Exists)
{
    idp.WithEnvironment("WEBAPP_HTTPS_CONTAINERHOST", webAppHttps);
    idp.WithEnvironment("WEBAPP_HTTPS", () => $"{webAppHttps.Scheme}://{webAppHttps.Host}:{webAppHttps.Port}");
}
else
{
    // Still need to set these environment variables so the KeyCloak realm import doesn't fail
    idp.WithEnvironment("WEBAPP_HTTPS_CONTAINERHOST", webAppHttp);
    idp.WithEnvironment("WEBAPP_HTTPS", () => $"{webAppHttp.Scheme}://{webAppHttp.Host}:{webAppHttp.Port}");
}

// Inject assigned URLs for Catalog API
catalogApi.WithEnvironment("CatalogOptions__PicBaseAddress", catalogApi.GetEndpoint("http"));

builder.Build().Run();
