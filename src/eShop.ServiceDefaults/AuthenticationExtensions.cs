using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery.Abstractions;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Extensions.ServiceDiscovery.Http;
using Grpc.Net.Client.Balancer;
using System.Threading;

namespace eShop.ServiceDefaults;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddDefaultAuthentication(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // {
        //   "Identity": {
        //     "Url": "http://identity",
        //     "Audience": "basket"
        //    }
        // }

        //var identitySection = configuration.GetSection("Identity");

        //if (!identitySection.Exists())
        //{
        //    // No identity section, so no authentication
        //    return services;
        //}

        // Prevent from mapping "sub" claim to nameidentifier.
        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        services.AddAuthentication()
            .AddJwtBearer();
        //    .AddJwtBearer(options =>
        //{
        //    //var identityUrl = identitySection.GetRequiredValue("Url");
        //    var realm = identitySection["Realm"] ?? "eShop";
        //    var identityUrl = $"http://idp/realms/{realm}";
        //    var audience = identitySection.GetRequiredValue("Audience");

        //    options.Authority = identityUrl;
        //    options.RequireHttpsMetadata = false;
        //    options.Audience = audience;
        //    options.TokenValidationParameters.ValidateAudience = false;
        //});

        //services.ConfigureOptions<ConfigureJwtBearer>();
        services.AddTransient<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearer>();
        services.AddTransient<IConfigureNamedOptions<JwtBearerOptions>, ConfigureJwtBearer>();

        services.AddAuthorization();

        return services;
    }

    private class ConfigureJwtBearer(
        IConfiguration configuration,
        ServiceEndPointResolverFactory resolverProvider,
        IServiceEndPointSelectorProvider selectorProvider,
        TimeProvider timeProvider)
        : IConfigureOptions<JwtBearerOptions>, IConfigureNamedOptions<JwtBearerOptions>
    {
        public void Configure(string? name, JwtBearerOptions options) => Configure(options);

        public void Configure(JwtBearerOptions options)
        {
            var identitySection = configuration.GetSection("Identity");

            if (!identitySection.Exists())
            {
                // No identity section, so no authentication
                return;
            }

            var realm = identitySection["Realm"] ?? "eShop";
            var identityUri = new Uri($"http://keycloak/realms/{realm}");
            var audience = identitySection.GetRequiredValue("Audience");
            var resolver = new HttpServiceEndPointResolver(resolverProvider, selectorProvider, timeProvider);
            var httpHandler = new ResolvingHttpClientHandler(resolver);
            var resolvedIdentityUri = resolver.ResolveUriAsync(identityUri).GetAwaiter().GetResult();

            options.BackchannelHttpHandler = httpHandler;
            options.Authority = resolvedIdentityUri.ToString();
            options.RequireHttpsMetadata = false;
            options.Audience = audience;
            options.TokenValidationParameters.ValidateAudience = false;
        }
    }
}
