using Lazy.Captcha.Core;
using RuoYi.Admin.Services;
using StackExchange.Profiling;

namespace RuoYi.Admin.Endpoints;

public static class CommonEndpoints
{
    public static IEndpointRouteBuilder MapCommonEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/captchaImage", GetCaptchaImage).AllowAnonymous();
        endpoints.MapGet("/GetDescription", GetDescription);
        endpoints.MapGet("/GetMiniProfilerIncludeScript", GetMiniProfilerIncludeScript);
        return endpoints;
    }

    private static object GetCaptchaImage(SysConfigService sysConfigService, ICaptcha captcha)
    {
        var captchaEnabled = sysConfigService.IsCaptchaEnabled();
        if (!captchaEnabled)
        {
            return new { CaptchaEnabled = false };
        }

        var uuid = Guid.NewGuid().ToString();
        var info = captcha.Generate(uuid);

        return new
        {
            Uuid = uuid,
            Img = info.Base64
        };
    }

    private static string GetDescription(SystemService systemService, ILogger<CommonEndpoints> logger)
    {
        logger.LogInformation("获取系统描述");
        return systemService.GetDescription();
    }

    private static string GetMiniProfilerIncludeScript(IHttpContextAccessor httpContextAccessor)
    {
        return MiniProfiler.Current.RenderIncludes(httpContextAccessor.HttpContext).Value;
    }
}
