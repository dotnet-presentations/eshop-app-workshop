using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Databases

var basketStore = builder.AddRedis("BasketStore")
    .WithRedisCommander();
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();
var catalogDb = postgres.AddDatabase("CatalogDB");
var orderDb = postgres.AddDatabase("OrderingDB");

// Identity Providers

var idp = builder.AddKeycloakContainer("idp", tag: "23.0")
    .WithExternalHttpEndpoints()
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

var webApp = builder.AddProject<WebApp>("webapp")
    .WithExternalHttpEndpoints()
    .WithReference(basketApi)
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(idp, env: "Identity__ClientSecret");

// Inject the project URLs for Keycloak realm configuration
idp.WithEnvironment(context =>
{
    var webAppHttp = webApp.GetEndpoint("http");
    var webAppHttps = webApp.GetEndpoint("https");

    var httpsEndpoint = webAppHttps.Exists ? webAppHttps : webAppHttp;

    context.EnvironmentVariables["WEBAPP_HTTP_CONTAINERHOST"] = webAppHttp;
    context.EnvironmentVariables["WEBAPP_HTTP"] = context.ExecutionContext.IsPublishMode ? webAppHttp : webAppHttp.Url;

    // Still need to set these environment variables so the KeyCloak realm import doesn't fail
    context.EnvironmentVariables["WEBAPP_HTTPS_CONTAINERHOST"] = httpsEndpoint;
    context.EnvironmentVariables["WEBAPP_HTTPS"] = context.ExecutionContext.IsPublishMode ? httpsEndpoint : httpsEndpoint.Url;
});

idp.WithEnvironment("ORDERINGAPI_HTTP", orderingApi.GetEndpoint("http"));

// Inject assigned URLs for Catalog API
catalogApi.WithEnvironment("CatalogOptions__PicBaseAddress", catalogApi.GetEndpoint("http"));

builder.Build().Run();
