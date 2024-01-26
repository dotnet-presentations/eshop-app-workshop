using eShop.Ordering.API.Data;
using eShop.Ordering.API.Infrastructure.Services;

namespace Microsoft.Extensions.Hosting;

internal static class HostingExtensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        
        // Add the authentication services to DI
        builder.AddDefaultAuthentication();

        builder.AddNpgsqlDbContext<OrderingDbContext>("OrderingDB");

        services.AddMigration<OrderingDbContext, OrderingDbContextSeed>();

        services.AddHttpContextAccessor();
        services.AddTransient<IIdentityService, IdentityService>();
    }
}
