using eShop.WebApp.Services;

namespace Microsoft.Extensions.Hosting;

public static class HostingExtensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpForwarderWithServiceDiscovery();

        // Application services
        builder.Services.AddSingleton<IProductImageUrlProvider, ProductImageUrlProvider>();
        builder.Services.AddScoped<LogOutService>();

        // HTTP and gRPC client registrations
        builder.Services.AddHttpClient<CatalogService>(o => o.BaseAddress = new("http://catalog-api"));
    }
}
