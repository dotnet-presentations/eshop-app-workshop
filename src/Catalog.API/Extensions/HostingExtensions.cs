using eShop.Catalog.API;
using eShop.Catalog.API.Infrastructure;

namespace Microsoft.Extensions.Hosting;

public static class HostingExtensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<CatalogDbContext>("CatalogDB");

        // TODO: Move this to a CatalogDbManager project
        builder.Services.AddMigration<CatalogDbContext, CatalogContextSeed>();

        builder.Services.Configure<CatalogOptions>(builder.Configuration.GetSection(nameof(CatalogOptions)));
    }
}
