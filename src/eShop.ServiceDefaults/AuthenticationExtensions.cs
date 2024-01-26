using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        // Prevent from mapping "sub" claim to nameidentifier.
        //JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");


        builder.Services.AddHttpClient(JwtBearerBackchannel, o => o.BaseAddress = new("http://keycloak"));

        services.AddAuthentication()
            .AddJwtBearer()
            .ConfigureDefaultJwtBearer();

        services.AddAuthorization();

        return services;
    }

    public static string GetIdpAuthorityUrl(this IHttpClientFactory httpClientFactory, IConfiguration configuration, string httpClientName)
    {
        var identitySection = configuration.GetSection("Identity");
        var backchannelClient = httpClientFactory.CreateClient(httpClientName);
        var realm = identitySection["Realm"] ?? "eShop";
        var identityUri = new Uri(
            backchannelClient.BaseAddress ?? throw new InvalidOperationException("OIDC backchannel HttpClient.BaseAddress not configured."),
            $"/realms/{realm}");

        return identityUri.ToString();
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

            options.Backchannel = httpClientFactory.CreateClient(JwtBearerBackchannel);
            options.Authority = httpClientFactory.GetIdpAuthorityUrl(configuration, JwtBearerBackchannel);
            options.RequireHttpsMetadata = false;
            options.Audience = audience;
            options.TokenValidationParameters.ValidateAudience = false;
            // Prevent from mapping "sub" claim to nameidentifier.
            options.MapInboundClaims = false;
        }
    }
}
