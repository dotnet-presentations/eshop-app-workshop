namespace Microsoft.Extensions.Hosting;

public static class HostingExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddRedis("BasketStore");

        return builder;
    }
}
