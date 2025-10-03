using Lms.Api.Extensions;
using Microsoft.AspNetCore.Http;

namespace Lms.Api.Filters;

public sealed class RoleAuthorizationFilter(params string[] requiredRoles) : IEndpointFilter
{
    private readonly string[] _requiredRoles = requiredRoles ?? Array.Empty<string>();

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        if (!httpContext.Request.Headers.TryGetValue("X-User-Role", out var roles) || string.IsNullOrWhiteSpace(roles))
        {
            return Results.Unauthorized();
        }

        var currentRole = roles.ToString();
        httpContext.SetUserRole(currentRole);

        if (httpContext.Request.Headers.TryGetValue("X-User-Id", out var userIdValue) && Guid.TryParse(userIdValue, out var userId))
        {
            httpContext.SetUserId(userId);
        }

        if (_requiredRoles.Length > 0 && !_requiredRoles.Contains(currentRole, StringComparer.OrdinalIgnoreCase))
        {
            return Results.Forbid();
        }

        return await next(context);
    }
}
