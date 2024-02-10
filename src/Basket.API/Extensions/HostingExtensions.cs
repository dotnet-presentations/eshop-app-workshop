﻿using eShop.Basket.API.Storage;

namespace Microsoft.Extensions.Hosting;

public static class HostingExtensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultAuthentication();

        builder.AddRedis("BasketStore");

        builder.Services.AddSingleton<IBasketStore, RedisBasketStore>();
    }
}
