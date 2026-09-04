using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

namespace RuoYi.Framework.Authorization;

/// <summary>
/// Minimal API endpoint authorization helpers.
/// Reuses the existing application authorization attributes and policy provider.
/// </summary>
public static class EndpointAuthorizationExtensions
{
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithMetadata(new AppAuthorizeAttribute(permissions));
        return builder;
    }

    public static TBuilder RequireRole<TBuilder>(this TBuilder builder, params string[] roles)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithMetadata(new AppRoleAuthorizeAttribute(roles));
        return builder;
    }
}
