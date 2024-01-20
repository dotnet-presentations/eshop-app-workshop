var builder = DistributedApplication.CreateBuilder(args);

// Databases & Message Brokers

var rabbitMq = builder.AddRabbitMQContainer("EventBus");
var redis = builder.AddRedis("redis");
var postgres = builder.AddPostgres("postgres");
var catalogDb = postgres.AddDatabase("CatalogDB");
var orderDb = postgres.AddDatabase("OrderingDB");

// Identity Providers

var keycloak = builder.AddKeycloakContainer("keycloak", tag: "23.0")
    .ImportRealms("../Keycloak/data/import");

// API Apps

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(rabbitMq)
    .WithReference(catalogDb);

var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithReference(keycloak);
    //.WithEnvironment("Identity__Url", GetKeycloakRealmUrl);

var orderingApi = builder.AddProject<Projects.Ordering_API>("ordering-api")
    .WithReference(rabbitMq)
    .WithReference(orderDb)
    .WithReference(keycloak);
    //.WithEnvironment("Identity__Url", GetKeycloakRealmUrl);

// Apps

var webApp = builder.AddProject<Projects.WebApp>("webapp")
    .WithReference(basketApi)
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(rabbitMq)
    .WithReference(keycloak)
    //.WithEnvironment("IdentityUrl", GetKeycloakRealmUrl)
    // Force HTTPS profile for web app (required for OIDC operations)
    .WithLaunchProfile("https");

// Wire up the callback URLs (self-referencing)
webApp.WithEnvironment("CallBackUrl", webApp.GetEndpoint("https"));

builder.Build().Run();

//string GetKeycloakRealmUrl()
//{
//    var baseUri = new Uri(idp.GetEndpoint("http").UriString);
//    var realmUri = new Uri(baseUri, "/realms/eShop");
//    return realmUri.ToString();
//}
