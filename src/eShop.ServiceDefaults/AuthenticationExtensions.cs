using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Microsoft.Extensions.Hosting;

public static class AuthenticationExtensions
{
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


        builder.Services.AddHttpClient("JwtBearerBackchannel", o => o.BaseAddress = new("http://keycloak"));

        services.AddAuthentication()
            .AddJwtBearer()
            .ConfigureDefaultJwtBearer();

        services.AddAuthorization();

        return services;
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
            var backchannelClient = httpClientFactory.CreateClient("JwtBearerBackchannel");
            var realm = identitySection["Realm"] ?? "eShop";
            var identityUri = new Uri(backchannelClient.BaseAddress!, $"/realms/{realm}");

            options.Backchannel = httpClientFactory.CreateClient();
            options.Authority = identityUri.ToString();
            options.RequireHttpsMetadata = false;
            options.Audience = audience;
            options.TokenValidationParameters.ValidateAudience = false;
            // Prevent from mapping "sub" claim to nameidentifier.
            options.MapInboundClaims = false;
        }
    }
}
