using RuoYi.Common.Utils;
using RuoYi.Data.Models;
using RuoYi.System.Services;

namespace RuoYi.Admin.Endpoints;

/// <summary>
/// System authentication and current-user endpoints implemented with ASP.NET Core Minimal API.
/// </summary>
public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/login", LoginAsync);

        var authorized = endpoints.MapGroup(string.Empty).RequireAuthorization();
        authorized.MapPost("/logout", Logout);
        authorized.MapGet("/getInfo", GetInfoAsync);
        authorized.MapGet("/getRouters", GetRouters);

        return endpoints;
    }

    private static async Task<AjaxResult> LoginAsync(
        LoginBody loginBody,
        SysLoginService sysLoginService)
    {
        var token = await sysLoginService.LoginAsync(
            loginBody.Username,
            loginBody.Password,
            loginBody.Code,
            loginBody.Uuid);

        var result = AjaxResult.Success();
        result.Add(Constants.TOKEN, token);
        return result;
    }

    private static AjaxResult Logout(
        HttpContext httpContext,
        TokenService tokenService,
        SysLogininforService sysLogininforService)
    {
        var loginUser = tokenService.GetLoginUser(httpContext.Request);
        if (loginUser != null)
        {
            var userName = loginUser.UserName;
            tokenService.DelLoginUser(loginUser.Token);
            _ = Task.Run(async () =>
            {
                await sysLogininforService.AddAsync(userName, Constants.LOGOUT, "退出成功");
            });
        }

        return AjaxResult.Success("退出成功");
    }

    private static async Task<AjaxResult> GetInfoAsync(
        SysPermissionService sysPermissionService)
    {
        var user = SecurityUtils.GetLoginUser().User;
        var roles = await sysPermissionService.GetRolePermissionAsync(user);
        var permissions = sysPermissionService.GetMenuPermission(user);

        var result = AjaxResult.Success();
        result.Add("user", user);
        result.Add("roles", roles);
        result.Add("permissions", permissions);
        return result;
    }

    private static AjaxResult GetRouters(SysMenuService sysMenuService)
    {
        var userId = SecurityUtils.GetUserId();
        var menus = sysMenuService.SelectMenuTreeByUserId(userId);
        var treeMenus = sysMenuService.BuildMenus(menus);
        return AjaxResult.Success(treeMenus);
    }
}
