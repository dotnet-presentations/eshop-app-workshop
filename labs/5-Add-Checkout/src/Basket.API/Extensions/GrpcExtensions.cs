using System.Security.Claims;

namespace Grpc.Core;

internal static class GrpcExtensions
{
    public static string? GetUserIdentity(this ServerCallContext context) => context.GetHttpContext().User.GetUserId();
}