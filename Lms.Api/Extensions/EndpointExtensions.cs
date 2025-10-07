using Lms.Api.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lms.Api.Extensions;

public static class EndpointExtensions
{
    private const string UserRoleItemKey = "user-role";
    private const string UserIdItemKey = "user-id";

    public static RouteHandlerBuilder RequireRoles(this RouteHandlerBuilder builder, params string[] roles)
    {
        return builder.AddEndpointFilter(new RoleAuthorizationFilter(roles));
    }

    public static string? GetUserRole(this HttpContext context)
    {
        if (context.Items.TryGetValue(UserRoleItemKey, out var value))
        {
            return value as string;
        }

        return null;
    }

    public static void SetUserRole(this HttpContext context, string role)
    {
        context.Items[UserRoleItemKey] = role;
    }

    public static Guid? GetUserId(this HttpContext context)
    {
        if (context.Items.TryGetValue(UserIdItemKey, out var value) && value is Guid guid)
        {
            return guid;
        }

        return null;
    }

    public static void SetUserId(this HttpContext context, Guid userId)
    {
        context.Items[UserIdItemKey] = userId;
    }
}
