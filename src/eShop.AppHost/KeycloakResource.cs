using Aspire.Hosting.Publishing;

namespace Aspire.Hosting;

internal static class KeycloakHostingExtensions
{
    private const int DefaultContainerPort = 8080;

    public static IResourceBuilder<TResource> WithReference<TResource>(this IResourceBuilder<TResource> builder,
        IResourceBuilder<KeycloakContainerResource> keycloakBuilder,
        string env)
        where TResource : IResourceWithEnvironment
    {
        builder.WithReference(keycloakBuilder);
        builder.WithEnvironment(env, keycloakBuilder.Resource.ClientSecret);

        return builder;
    }

    public static IResourceBuilder<KeycloakContainerResource> AddKeycloakContainer(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null,
        string? tag = null)
    {
        var keycloakContainer = new KeycloakContainerResource(name)
        {
            ClientSecret = Guid.NewGuid().ToString("N")
        };

        return builder
            .AddResource(keycloakContainer)
            .WithAnnotation(new ContainerImageAnnotation { Registry = "quay.io", Image = "keycloak/keycloak", Tag = tag ?? "latest" })
            .WithHttpEndpoint(port: port, targetPort: DefaultContainerPort)
            .WithEnvironment("KEYCLOAK_ADMIN", "admin")
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
            .WithEnvironment("WEBAPP_CLIENT_SECRET", keycloakContainer.ClientSecret)
            .WithArgs("start-dev")
            .WithManifestPublishingCallback(context => WriteKeycloakContainerToManifest(context, keycloakContainer));
    }

    public static IResourceBuilder<KeycloakContainerResource> ImportRealms(this IResourceBuilder<KeycloakContainerResource> builder, string source)
    {
        builder
            .WithBindMount(source, "/opt/keycloak/data/import")
            .WithAnnotation(new CommandLineArgsCallbackAnnotation(args =>
            {
                args.Clear();
                args.Add("start-dev");
                args.Add("--import-realm");
            }));

        return builder;
    }

    private static async Task WriteKeycloakContainerToManifest(ManifestPublishingContext context, KeycloakContainerResource resource)
    {
        var manifestResource = new KeycloakContainerResource(resource.Name);

        foreach (var annotation in resource.Annotations)
        {
            if (annotation is not CommandLineArgsCallbackAnnotation)
            {
                manifestResource.Annotations.Add(annotation);
            }
        }

        // Set the container entry point to 'start' instead of 'start-dev'
        manifestResource.Annotations.Add(new CommandLineArgsCallbackAnnotation(args => args.Add("start")));

        await context.WriteContainerAsync(resource);
    }
}

internal class KeycloakContainerResource(string name) : ContainerResource(name), IResourceWithServiceDiscovery
{
    public string? ClientSecret { get; set; }
}
