using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using eShop.WebApp.Services;
using eShop.Basket.API.Grpc;

namespace Microsoft.Extensions.Hosting;

public static class HostingExtensions
{
    public const string OpenIdConnectBackchannel = "OpenIdConnectBackchannel";

    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddAuthenticationServices();

        builder.Services.AddHttpForwarderWithServiceDiscovery();

        // Application services
        builder.Services.AddScoped<BasketState>();
        builder.Services.AddScoped<LogOutService>();
        builder.Services.AddSingleton<BasketService>();
        builder.Services.AddSingleton<IProductImageUrlProvider, ProductImageUrlProvider>();

        // HTTP and gRPC client registrations
        builder.Services.AddGrpcClient<Basket.BasketClient>(o => o.Address = new("http://basket-api"))
            .AddAuthToken();

        builder.Services.AddHttpClient<CatalogService>(o => o.BaseAddress = new("http://catalog-api"));

        builder.Services.AddHttpClient(OpenIdConnectBackchannel, o => o.BaseAddress = new("http://idp"));
    }

    public static void AddAuthenticationServices(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var services = builder.Services;

        var sessionCookieLifetime = configuration.GetValue("SessionCookieLifetimeMinutes", 60);

        // Add Authentication services
        services.AddAuthorization();
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
            .AddCookie(options => options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionCookieLifetime))
            .AddOpenIdConnect()
            .ConfigureWebAppOpenIdConnect();

        // Blazor auth services
        services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        services.AddCascadingAuthenticationState();
    }

    private static void ConfigureWebAppOpenIdConnect(this AuthenticationBuilder authentication)
    {
        // Named options
        authentication.Services.AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
            .Configure<IConfiguration, IHttpClientFactory, IHostEnvironment>(configure);

        // Unnamed options
        authentication.Services.AddOptions<OpenIdConnectOptions>()
            .Configure<IConfiguration, IHttpClientFactory, IHostEnvironment>(configure);

        static void configure(OpenIdConnectOptions options, IConfiguration configuration, IHttpClientFactory httpClientFactory, IHostEnvironment hostEnvironment)
        {
            var clientSecret = configuration.GetRequiredSection("Identity").GetRequiredValue("ClientSecret");
            var backchannelHttpClient = httpClientFactory.CreateClient(OpenIdConnectBackchannel);

            options.Backchannel = backchannelHttpClient;
            options.Authority = backchannelHttpClient.GetIdpAuthorityUri(configuration).ToString();
            options.ClientId = "webapp";
            options.ClientSecret = clientSecret;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.SaveTokens = true; // Preserve the access token so it can be used to call backend APIs
            options.RequireHttpsMetadata = !hostEnvironment.IsDevelopment();
            options.MapInboundClaims = false; // Prevent from mapping "sub" claim to nameidentifier.
        }
    }

    public static async Task<string?> GetBuyerIdAsync(this AuthenticationStateProvider authenticationStateProvider)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.GetUserId();
    }

    public static async Task<string?> GetUserNameAsync(this AuthenticationStateProvider authenticationStateProvider)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.GetUserName();
    }
}
