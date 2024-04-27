using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.Hosting;

public static class OpenApiExtensions
{
    public static IApplicationBuilder UseDefaultOpenApi(this WebApplication app)
    {
        var configuration = app.Configuration;
        var openApiSection = configuration.GetSection("OpenApi");

        if (!openApiSection.Exists())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(setup =>
        {
            /// {
            ///   "OpenApi": {
            ///     "Endpoint: {
            ///         "Name": 
            ///     },
            ///     "Auth": {
            ///         "ClientId": ..,
            ///         "AppName": ..
            ///     }
            ///   }
            /// }

            var pathBase = configuration["PATH_BASE"];
            var authSection = openApiSection.GetSection("Auth");
            var endpointSection = openApiSection.GetRequiredSection("Endpoint");

            var swaggerUrl = endpointSection["Url"] ?? $"{pathBase}/swagger/v1/swagger.json";

            setup.SwaggerEndpoint(swaggerUrl, endpointSection.GetRequiredValue("Name"));

            if (authSection.Exists())
            {
                setup.OAuthClientId(authSection.GetRequiredValue("ClientId"));
                setup.OAuthAppName(authSection.GetRequiredValue("AppName"));
            }
        });

        // Add a redirect from the root of the app to the swagger endpoint
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

        return app;
    }

    public static IHostApplicationBuilder AddDefaultOpenApi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        var openApi = configuration.GetSection("OpenApi");

        if (!openApi.Exists())
        {
            return builder;
        }

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen();

        services.AddOptions<SwaggerGenOptions>()
            .Configure<ServiceEndpointResolver>((options, ServiceEndpointResolver) =>
            {
                /// {
                ///   "OpenApi": {
                ///     "Document": {
                ///         "Title": ..
                ///         "Version": ..
                ///         "Description": ..
                ///     }
                ///   }
                /// }
                var document = openApi.GetRequiredSection("Document");

                var version = document.GetRequiredValue("Version") ?? "v1";

                options.SwaggerDoc(version, new()
                {
                    Title = document.GetRequiredValue("Title"),
                    Version = version,
                    Description = document.GetRequiredValue("Description")
                });

                var identitySection = configuration.GetSection("Identity");

                if (!identitySection.Exists())
                {
                    // No identity section, so no authentication OpenAPI definition
                    return;
                }

                // {
                //   "Identity": {
                //     "Audience": "orders",
                //     "Scopes": {
                //         "basket": "Basket API"
                //      }
                //    }
                // }

                var identityUri = ServiceEndpointResolver.ResolveIdpAuthorityUri(configuration);

                var scopes = identitySection.GetSection("Scopes").GetChildren().ToDictionary(p => p.Key, p => p.Value);

                options.AddSecurityDefinition("oauth2", new()
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        // TODO: Change this to use Authorization Code flow with PKCE
                        Implicit = new()
                        {
                            AuthorizationUrl = new Uri(identityUri, "protocol/openid-connect/auth"),
                            TokenUrl = new Uri(identityUri, "protocol/openid-connect/token"),
                            Scopes = scopes,
                        }
                    }
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>([scopes.Keys.ToArray()]);
            });

        return builder;
    }

    private sealed class AuthorizeCheckOperationFilter(string[] scopes) : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

            if (!metadata.OfType<IAuthorizeData>().Any())
            {
                return;
            }

            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

            var oAuthScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            };

            operation.Security =
            [
                new()
                {
                    [ oAuthScheme ] = scopes
                }
            ];
        }
    }
}
