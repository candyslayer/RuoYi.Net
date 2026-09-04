using Microsoft.AspNetCore.Authorization;

namespace RuoYi.Admin.Authorization;

/// <summary>
/// Minimal API endpoint authorization helpers.
/// Reuses the existing AppAuthorizeAttribute and authorization middleware.
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
