using System.Text.Json.Serialization;
using eShop.Basket.API.Storage;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultAuthentication();

        builder.AddRedis("basketStore");

        builder.Services.AddSingleton<IBasketStore, RedisBasketStore>();
    }
}
