using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ServiceDiscovery;

namespace Microsoft.Extensions.Hosting;

public static class AuthenticationExtensions
{
    public const string JwtBearerBackchannel = "JwtBearerBackchannel";

    public static IServiceCollection AddDefaultAuthentication(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // {
        //   "Identity": {
        //     "Audience": "basket"
        //    }
        // }

        builder.Services.AddHttpClient(JwtBearerBackchannel, o => o.BaseAddress = new("http://idp"));

        services.AddAuthentication()
            .AddJwtBearer()
            .ConfigureDefaultJwtBearer();

        services.AddAuthorization();

        return services;
    }

    public static Uri GetIdpAuthorityUri(this HttpClient httpClient, IConfiguration configuration)
    {
        var idpBaseUri = httpClient.BaseAddress
            ?? throw new InvalidOperationException($"HttpClient instance does not have a BaseAddress configured.");
        var identityUri = GetIdpRealmUri(idpBaseUri, configuration);

        return identityUri;
    }

    public static Uri ResolveIdpAuthorityUri(this ServiceEndpointResolver resolver, IConfiguration configuration, string serviceName = "http://idp")
    {
        // Sync over async :(
        var idpBaseUrl = resolver.ResolveEndPointUrlAsync(serviceName).AsTask().GetAwaiter().GetResult()
            ?? throw new InvalidOperationException($"Could not resolve IdP address using service name '{serviceName}'.");
        var identityUri = GetIdpRealmUri(new Uri(idpBaseUrl), configuration);

        return identityUri;
    }

    private static Uri GetIdpRealmUri(Uri idpBaseUri, IConfiguration configuration)
    {
        var identitySection = configuration.GetSection("Identity");
        var realm = identitySection["Realm"] ?? "eShop";

        return new Uri(idpBaseUri, $"realms/{realm}/");
    }

    private static void ConfigureDefaultJwtBearer(this AuthenticationBuilder authentication)
    {
        // Named options
        authentication.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IConfiguration, IHttpClientFactory>(configure);

        // Unnamed options
        authentication.Services.AddOptions<JwtBearerOptions>()
            .Configure<IConfiguration, IHttpClientFactory>(configure);

        static void configure(JwtBearerOptions options, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            var identitySection = configuration.GetSection("Identity");
            var audience = identitySection.GetRequiredValue("Audience");
            var backchannelHttpClient = httpClientFactory.CreateClient(JwtBearerBackchannel);

            options.Backchannel = backchannelHttpClient;
            options.Authority = backchannelHttpClient.GetIdpAuthorityUri(configuration).ToString();
            options.RequireHttpsMetadata = false;
            options.Audience = audience;
            options.TokenValidationParameters.ValidateAudience = false;
            // Prevent from mapping "sub" claim to nameidentifier.
            options.MapInboundClaims = false;
        }
    }
}
