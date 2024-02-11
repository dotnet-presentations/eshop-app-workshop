using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using eShop.Ordering.API.Model;
using eShop.Ordering.Data;

namespace Microsoft.AspNetCore.Builder;

public static class OrderingApi
{
    public static RouteGroupBuilder MapOrdersApi(this RouteGroupBuilder app)
    {
        app.MapGet("/", async (ClaimsPrincipal user, OrderingDbContext dbContext) =>
        {
            var userId = user.GetUserId()
                ?? throw new InvalidOperationException("User identity could not be found. This endpoint requires authorization.");

            var orders = await dbContext.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Buyer)
                .Where(o => o.Buyer.IdentityGuid == userId)
                .Select(o => OrderSummary.FromOrder(o))
                .AsNoTracking()
                .ToArrayAsync();

            return TypedResults.Ok(orders);
        });

        return app;
    }
}
