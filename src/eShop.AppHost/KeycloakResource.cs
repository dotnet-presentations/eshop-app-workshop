using Aspire.Hosting.Publishing;

namespace Aspire.Hosting;

internal static class KeycloakHostingExtensions
{
    private const int DefaultContainerPort = 8080;

    public static IResourceBuilder<KeycloakContainerResource> AddKeycloakContainer(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null,
        string? tag = null)
    {
        var keycloakContainer = new KeycloakContainerResource(name);

        return builder
            .AddResource(keycloakContainer)
            .WithAnnotation(new ContainerImageAnnotation { Registry = "quay.io", Image = "keycloak/keycloak", Tag = tag ?? "latest" })
            .WithHttpEndpoint(hostPort: port, containerPort: DefaultContainerPort)
            .WithEnvironment("KEYCLOAK_ADMIN", "admin")
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
            .WithArgs("start-dev")
            .WithManifestPublishingCallback(context => WriteKeycloakContainerToManifest(context, keycloakContainer));
    }

    public static IResourceBuilder<KeycloakContainerResource> ImportRealms(this IResourceBuilder<KeycloakContainerResource> builder, string source)
    {
        builder
            .WithVolumeMount(source, "/opt/keycloak/data/import", VolumeMountType.Bind)
            .WithAnnotation(new ExecutableArgsCallbackAnnotation(args =>
            {
                args.Clear();
                args.Add("start-dev");
                args.Add("--import-realm");
            }));

        return builder;
    }

    private static void WriteKeycloakContainerToManifest(ManifestPublishingContext context, KeycloakContainerResource resource)
    {
        var manifestResource = new KeycloakContainerResource(resource.Name);

        foreach (var annotation in resource.Annotations)
        {
            if (annotation is not ExecutableArgsCallbackAnnotation)
            {
                manifestResource.Annotations.Add(annotation);
            }
        }

        // Set the container entry point to 'start' instead of 'start-dev'
        manifestResource.Annotations.Add(new ExecutableArgsCallbackAnnotation(args => args.Add("start")));

        context.WriteContainer(resource);
    }
}

internal class KeycloakContainerResource(string name) : ContainerResource(name), IResourceWithServiceDiscovery
{
}
