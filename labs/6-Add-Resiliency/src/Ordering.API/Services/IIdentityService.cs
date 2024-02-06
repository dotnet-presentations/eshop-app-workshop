namespace eShop.Ordering.API.Services;

public interface IIdentityService
{
    string? GetUserIdentity();

    string? GetUserName();
}

