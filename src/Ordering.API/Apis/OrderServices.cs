using eShop.Ordering.API.Data;
using eShop.Ordering.API.Infrastructure.Services;

namespace eShop.Ordering.API.Apis;

public class OrderServices(OrderingDbContext dbContext, IIdentityService identityService, ILogger<OrderServices> logger)
{
    public OrderingDbContext DbContext { get; } = dbContext;

    public IIdentityService IdentityService { get; } = identityService;

    public ILogger<OrderServices> Logger { get; } = logger;
}