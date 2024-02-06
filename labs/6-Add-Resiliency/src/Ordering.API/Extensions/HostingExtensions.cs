using eShop.Ordering.Data;
using eShop.Ordering.API.Services;

namespace Microsoft.Extensions.Hosting;

internal static class HostingExtensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        
        // Add the authentication services to DI
        builder.AddDefaultAuthentication();

        builder.AddNpgsqlDbContext<OrderingDbContext>("OrderingDB");

        services.AddHttpContextAccessor();
        services.AddTransient<IIdentityService, IdentityService>();

        builder.Services.AddProblemDetails();
    }
}
