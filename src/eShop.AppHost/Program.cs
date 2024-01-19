using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedisContainer("redis");
var rabbitMq = builder.AddRabbitMQContainer("EventBus");
var postgres = builder.AddPostgresContainer("postgres");

var idp = builder.AddContainer("keycloak", image: "quay.io/keycloak/keycloak", tag: "latest")
    .WithServiceBinding(hostPort: 8080, containerPort: 8080, scheme: "http")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_HOSTNAME", "localhost")
    .WithVolumeMount("../Keycloak/data/import", "/opt/keycloak/data/import", VolumeMountType.Bind)
    // Keycloak defaults to "dev-file" for database storage but Postgres might be more desirable
    //.WithEnvironment("KC_DB", "postgres")
    //.WithEnvironment("KC_DB_URL", () => $"postgres://{new Uri(postgres.GetEndpoint("tcp").UriString).GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped)}")
    //.WithEnvironment("KC_DB_USERNAME", "postgres")
    //.WithEnvironment("KC_DB_PASSWORD", () => postgres.Resource.Password)
    .WithArgs("start-dev", "--import-realm");

var catalogDb = postgres.AddDatabase("CatalogDB");
//var identityDb = postgres.AddDatabase("IdentityDB");
var orderDb = postgres.AddDatabase("OrderingDB");
//var webhooksDb = postgres.AddDatabase("WebHooksDB");

// Services
//var identityApi = builder.AddProject<Projects.Identity_API>("identity-api")
//    .WithReference(identityDb);

var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
    .WithReference(redis)
    .WithReference(rabbitMq);
    //.WithEnvironment("Identity__Url", identityApi.GetEndpoint("http"));

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(rabbitMq)
    .WithReference(catalogDb);

var orderingApi = builder.AddProject<Projects.Ordering_API>("ordering-api")
    .WithReference(rabbitMq)
    .WithReference(orderDb);
    //.WithEnvironment("Identity__Url", identityApi.GetEndpoint("http"));

//builder.AddProject<Projects.OrderProcessor>("order-processor")
//    .WithReference(rabbitMq)
//    .WithReference(orderDb);

//builder.AddProject<Projects.PaymentProcessor>("payment-processor")
//    .WithReference(rabbitMq);

//var webHooksApi = builder.AddProject<Projects.Webhooks_API>("webhooks-api")
//    .WithReference(rabbitMq)
//    .WithReference(webhooksDb)
//    .WithEnvironment("Identity__Url", identityApi.GetEndpoint("http"));

// Reverse proxies
//builder.AddProject<Projects.Mobile_Bff_Shopping>("mobile-bff")
//    .WithReference(catalogApi)
//    .WithReference(identityApi);

// Apps
//var webhooksClient = builder.AddProject<Projects.WebhookClient>("webhooksclient")
//    .WithReference(webHooksApi)
//    .WithEnvironment("IdentityUrl", identityApi.GetEndpoint("http"));

var webApp = builder.AddProject<Projects.WebApp>("webapp")
    .WithReference(basketApi)
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(rabbitMq)
    //.WithEnvironment("IdentityUrl", identityApi.GetEndpoint("http"))
    .WithLaunchProfile("https");

// Wire up the callback urls (self referencing)
//webApp.WithEnvironment("CallBackUrl", webApp.GetEndpoint("https"));
//webhooksClient.WithEnvironment("CallBackUrl", webhooksClient.GetEndpoint("https"));

// Identity has a reference to all of the apps for callback urls, this is a cyclic reference
//identityApi.WithEnvironment("BasketApiClient", basketApi.GetEndpoint("http"))
//           .WithEnvironment("OrderingApiClient", orderingApi.GetEndpoint("http"))
//           .WithEnvironment("WebhooksApiClient", webHooksApi.GetEndpoint("http"))
//           .WithEnvironment("WebhooksWebClient", webhooksClient.GetEndpoint("https"))
//           .WithEnvironment("WebAppClient", webApp.GetEndpoint("https"));

builder.Build().Run();
