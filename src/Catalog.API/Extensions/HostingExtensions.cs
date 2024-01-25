using eShop.Catalog.API;
using eShop.Catalog.API.Infrastructure;

namespace Microsoft.Extensions.Hosting;

public static class HostingExtensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<CatalogContext>("CatalogDB");

        // TODO: Move this to a CatalogDbManager project
        builder.Services.AddMigration<CatalogContext, CatalogContextSeed>();

        builder.Services.Configure<CatalogOptions>(builder.Configuration.GetSection(nameof(CatalogOptions)));
    }
}
