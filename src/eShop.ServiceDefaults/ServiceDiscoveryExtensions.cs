using System.Net;
using Microsoft.Extensions.ServiceDiscovery.Http;

namespace eShop.ServiceDefaults;

public static class ServiceDiscoveryExtensions
{
    public static async Task<Uri> ResolveUriAsync(this HttpServiceEndPointResolver resolver, Uri originalUri)
    {
        // Copied from ResolvingHttpDelegatingHandler.GetUriWithEndPoint
        var dummyRequest = new HttpRequestMessage(HttpMethod.Get, originalUri);
        var endpoint = (await resolver.GetEndpointAsync(dummyRequest, default)).EndPoint;

        string host;
        int port;
        switch (endpoint)
        {
            case IPEndPoint ip:
                host = ip.Address.ToString();
                port = ip.Port;
                break;
            case DnsEndPoint dns:
                host = dns.Host;
                port = dns.Port;
                break;
            default:
                throw new InvalidOperationException($"Endpoints of type {endpoint.GetType()} are not supported");
        }

        var builder = new UriBuilder(originalUri)
        {
            Host = host,
        };

        // Default to the default port for the scheme.
        if (port > 0)
        {
            builder.Port = port;
        }

        return builder.Uri;
    }
}
