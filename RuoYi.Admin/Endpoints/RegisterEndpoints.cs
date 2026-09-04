using RuoYi.Common.Utils;
using RuoYi.Data.Models;
using RuoYi.System.Services;

namespace RuoYi.Admin.Endpoints;

public static class RegisterEndpoints
{
    public static IEndpointRouteBuilder MapRegisterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/register", RegisterAsync).AllowAnonymous();
        return endpoints;
    }

    private static async Task<AjaxResult> RegisterAsync(
        RegisterBody user,
        SysRegisterService registerService,
        SysConfigService configService)
    {
        if (!"true".Equals(configService.SelectConfigByKey("sys.account.registerUser")))
        {
            return AjaxResult.Error("当前系统没有开启注册功能！");
        }

        var msg = await registerService.RegisterAsync(user);
        return StringUtils.IsEmpty(msg) ? AjaxResult.Success() : AjaxResult.Error(msg);
    }
}
