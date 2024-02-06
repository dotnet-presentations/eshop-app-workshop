using System.Security.Claims;

namespace Grpc.Core;

internal static class ServerCallContextIdentityExtensions
{
    public static string? GetUserIdentity(this ServerCallContext context) => context.GetHttpContext().User.GetUserId();
    public static string? GetUserName(this ServerCallContext context) => context.GetHttpContext().User.GetUserName();
}
