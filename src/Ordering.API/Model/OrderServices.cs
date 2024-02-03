using eShop.Ordering.Data;
using eShop.Ordering.API.Services;

namespace eShop.Ordering.API.Model;

public class OrderServices(OrderingDbContext dbContext, IIdentityService identityService)
{
    public OrderingDbContext DbContext { get; } = dbContext;

    public IIdentityService IdentityService { get; } = identityService;
}