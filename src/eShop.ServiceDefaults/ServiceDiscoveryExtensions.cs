namespace Microsoft.Extensions.ServiceDiscovery;

public static class ServiceDiscoveryExtensions
{
    public static async ValueTask<string?> ResolveEndPointUrlAsync(this ServiceEndPointResolverRegistry resolver, string serviceName, CancellationToken cancellationToken = default)
    {
        var scheme = ExtractScheme(serviceName);
        var endpoints = await resolver.GetEndPointsAsync(serviceName, cancellationToken);
        if (endpoints.Count > 0)
        {
            var address = endpoints[0].GetEndPointString();
            return $"{scheme}://{address}";
        }
        return null;
    }

    private static string? ExtractScheme(string serviceName)
    {
        if (Uri.TryCreate(serviceName, UriKind.Absolute, out var uri))
        {
            return uri.Scheme;
        }
        return null;
    }
}
