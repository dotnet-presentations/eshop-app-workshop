using eShop.WebApp;
using eShop.WebAppComponents.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.Extensions.ServiceDiscovery.Abstractions;
using Microsoft.Extensions.ServiceDiscovery.Http;
using Microsoft.IdentityModel.JsonWebTokens;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddAuthenticationServices();

        //builder.AddRabbitMqEventBus("EventBus")
        //       .AddEventBusSubscriptions();

        builder.Services.AddHttpForwarderWithServiceDiscovery();

        // Application services
        builder.Services.AddScoped<BasketState>();
        builder.Services.AddScoped<LogOutService>();
        builder.Services.AddSingleton<BasketService>();
        builder.Services.AddSingleton<OrderStatusNotificationService>();
        builder.Services.AddSingleton<IProductImageUrlProvider, ProductImageUrlProvider>();

        // HTTP and gRPC client registrations
        builder.Services.AddGrpcClient<Basket.BasketClient>(o => o.Address = new("http://basket-api"))
            .AddAuthToken();

        builder.Services.AddHttpClient<CatalogService>(o => o.BaseAddress = new("http://catalog-api"))
            .AddAuthToken();

        builder.Services.AddHttpClient<OrderingService>(o => o.BaseAddress = new("http://ordering-api"))
            .AddAuthToken();
    }

    //public static void AddEventBusSubscriptions(this IEventBusBuilder eventBus)
    //{
    //    eventBus.AddSubscription<OrderStatusChangedToAwaitingValidationIntegrationEvent, OrderStatusChangedToAwaitingValidationIntegrationEventHandler>();
    //    eventBus.AddSubscription<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>();
    //    eventBus.AddSubscription<OrderStatusChangedToStockConfirmedIntegrationEvent, OrderStatusChangedToStockConfirmedIntegrationEventHandler>();
    //    eventBus.AddSubscription<OrderStatusChangedToShippedIntegrationEvent, OrderStatusChangedToShippedIntegrationEventHandler>();
    //    eventBus.AddSubscription<OrderStatusChangedToCancelledIntegrationEvent, OrderStatusChangedToCancelledIntegrationEventHandler>();
    //    eventBus.AddSubscription<OrderStatusChangedToSubmittedIntegrationEvent, OrderStatusChangedToSubmittedIntegrationEventHandler>();
    //}

    public static void AddAuthenticationServices(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var services = builder.Services;

        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        //var identityUrl = configuration.GetRequiredValue("IdentityUrl");
        //var callBackUrl = configuration.GetRequiredValue("CallBackUrl");
        var sessionCookieLifetime = configuration.GetValue("SessionCookieLifetimeMinutes", 60);

        // Add Authentication services
        services.AddAuthorization();
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options => options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionCookieLifetime))
        .AddOpenIdConnect(
            //options =>
            //{
            //    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    //options.Authority = identityUrl;
            //    //options.SignedOutRedirectUri = callBackUrl;
            //    options.ClientId = "webapp";
            //    options.ClientSecret = builder.Configuration.GetRequiredValue("ClientSecret");
            //    options.ResponseType = "code";
            //    options.SaveTokens = true;
            //    options.GetClaimsFromUserInfoEndpoint = true;
            //    options.RequireHttpsMetadata = false;
            //    //options.Scope.Add("openid");
            //    //options.Scope.Add("profile");
            //    options.Scope.Add("orders");
            //    options.Scope.Add("basket");
            //}
        );

        services.ConfigureOptions<ConfigureOidc>();
        //services.AddTransient<IConfigureOptions<OpenIdConnectOptions>, ConfigureOidc>();
        //services.AddTransient<IConfigureNamedOptions<OpenIdConnectOptions>, ConfigureOidc>();

        // Blazor auth services
        services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        services.AddCascadingAuthenticationState();
    }

    public static async Task<string?> GetBuyerIdAsync(this AuthenticationStateProvider authenticationStateProvider)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.FindFirst("sub")?.Value;
    }

    public static async Task<string?> GetUserNameAsync(this AuthenticationStateProvider authenticationStateProvider)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.FindFirst("name")?.Value;
    }

    private class ConfigureOidc(
        IConfiguration configuration,
        ServiceEndPointResolverFactory resolverProvider,
        IServiceEndPointSelectorProvider selectorProvider,
        TimeProvider timeProvider)
        : IConfigureOptions<OpenIdConnectOptions>, IConfigureNamedOptions<OpenIdConnectOptions>
    {
        public void Configure(string? name, OpenIdConnectOptions options) => Configure(options);

        public void Configure(OpenIdConnectOptions options)
        {
            var identitySection = configuration.GetSection("Identity");

            if (!identitySection.Exists())
            {
                // No identity section, so no authentication
                return;
            }

            var realm = identitySection["Realm"] ?? "eShop";
            var identityUri = new Uri($"http://keycloak/realms/{realm}");
            var callBackUrl = configuration.GetRequiredValue("CallBackUrl");
            var clientSecret = configuration.GetRequiredValue("ClientSecret");
            var resolver = new HttpServiceEndPointResolver(resolverProvider, selectorProvider, timeProvider);
            var httpHandler = new ResolvingHttpClientHandler(resolver);
            var resolvedIdentityUri = resolver.ResolveUriAsync(identityUri).GetAwaiter().GetResult();

            options.BackchannelHttpHandler = httpHandler;
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = resolvedIdentityUri.ToString();
            options.SignedOutRedirectUri = callBackUrl;
            options.ClientId = "webapp";
            options.ClientSecret = clientSecret;
            options.ResponseType = "code";
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.RequireHttpsMetadata = false;
            //options.Scope.Add("openid");
            //options.Scope.Add("profile");
            options.Scope.Add("orders");
            options.Scope.Add("basket");

            //options.Authority = identityUri.ToString();
            //options.RequireHttpsMetadata = false;
            //options.Audience = audience;
            //options.TokenValidationParameters.ValidateAudience = false;
        }
    }
}
