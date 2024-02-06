using System.Security.Claims;

namespace eShop.Ordering.API.Services;

public class IdentityService(IHttpContextAccessor context) : IIdentityService
{
    public string? GetUserIdentity()
        => context.HttpContext?.User.GetUserId();

    public string? GetUserName()
        => context.HttpContext?.User.GetUserName();
}
