var builder = DistributedApplication.CreateBuilder(args);

// Databases & Message Brokers

var rabbitMq = builder.AddRabbitMQContainer("EventBus");
var redis = builder.AddRedis("redis");
var postgres = builder.AddPostgres("postgres");
var catalogDb = postgres.AddDatabase("CatalogDB");
var orderDb = postgres.AddDatabase("OrderingDB");

// Identity Providers

var idp = builder.AddContainer("keycloak", image: "quay.io/keycloak/keycloak", tag: "23.0")
    .WithServiceBinding(hostPort: 8080, containerPort: 8080, scheme: "http")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_HOSTNAME", "localhost")
    .WithVolumeMount("../Keycloak/data/import", "/opt/keycloak/data/import", VolumeMountType.Bind)
    .WithArgs("start-dev", "--import-realm");

// API Apps

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(rabbitMq)
    .WithReference(catalogDb);

var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithEnvironment("Identity__Url", GetKeycloakRealmUrl);

var orderingApi = builder.AddProject<Projects.Ordering_API>("ordering-api")
    .WithReference(rabbitMq)
    .WithReference(orderDb)
    .WithEnvironment("Identity__Url", GetKeycloakRealmUrl);

// Apps

var webApp = builder.AddProject<Projects.WebApp>("webapp")
    .WithReference(basketApi)
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(rabbitMq)
    .WithEnvironment("IdentityUrl", GetKeycloakRealmUrl)
    // Force HTTPS profile for web app (required for OIDC operations)
    .WithLaunchProfile("https");

// Wire up the callback URLs (self-referencing)
webApp.WithEnvironment("CallBackUrl", webApp.GetEndpoint("https"));

builder.Build().Run();

string GetKeycloakRealmUrl()
{
    var baseUri = new Uri(idp.GetEndpoint("http").UriString);
    var realmUri = new Uri(baseUri, "/realms/eShop");
    return realmUri.ToString();
}
