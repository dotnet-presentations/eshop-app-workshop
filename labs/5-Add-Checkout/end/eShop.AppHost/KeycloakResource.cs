using Aspire.Hosting.Publishing;

namespace Aspire.Hosting;

internal static class KeycloakHostingExtensions
{
    private const int DefaultContainerPort = 8080;

    public static IResourceBuilder<TResource> WithReference<TResource>(this IResourceBuilder<TResource> builder,
        IResourceBuilder<KeycloakResource> keycloakBuilder,
        string env)
        where TResource : IResourceWithEnvironment
    {
        builder.WithReference(keycloakBuilder);
        builder.WithEnvironment(env, keycloakBuilder.Resource.ClientSecret);

        return builder;
    }

    public static IResourceBuilder<KeycloakResource> AddKeycloakContainer(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null,
        string? tag = null)
    {
        var keycloakContainer = new KeycloakResource(name)
        {
            ClientSecret = Guid.NewGuid().ToString("N")
        };

        var keycloak = builder
            .AddResource(keycloakContainer)
            .WithAnnotation(new ContainerImageAnnotation { Registry = "quay.io", Image = "keycloak/keycloak", Tag = tag ?? "latest" })
            .WithHttpEndpoint(port: port, targetPort: DefaultContainerPort)
            .WithEnvironment("KEYCLOAK_ADMIN", "admin")
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
            .WithEnvironment("WEBAPP_CLIENT_SECRET", keycloakContainer.ClientSecret);

        if (builder.ExecutionContext.IsRunMode)
        {
            keycloak.WithArgs("start-dev");
        }
        else
        {
            keycloak.WithArgs("start");
        }

        return keycloak;
    }

    public static IResourceBuilder<KeycloakResource> ImportRealms(this IResourceBuilder<KeycloakResource> builder, string source)
    {
        builder
            .WithBindMount(source, "/opt/keycloak/data/import")
            .WithAnnotation(new CommandLineArgsCallbackAnnotation(args =>
            {
                // TODO: This could be cleaned up to make it properly compose with any other callers who customize args
                args.Clear();
                if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
                {
                    args.Add("start-dev");
                }
                else
                {
                    args.Add("start");
                }
                args.Add("--import-realm");
            }));

        return builder;
    }
}

internal class KeycloakResource(string name) : ContainerResource(name), IResourceWithServiceDiscovery
{
    public string? ClientSecret { get; set; }
}
