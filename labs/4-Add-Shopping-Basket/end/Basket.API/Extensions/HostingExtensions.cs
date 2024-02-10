using eShop.Basket.API.Storage;

namespace Microsoft.Extensions.Hosting;

public static class HostingExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddRedis("BasketStore");

        builder.Services.AddSingleton<RedisBasketStore>();

        return builder;
    }
}
