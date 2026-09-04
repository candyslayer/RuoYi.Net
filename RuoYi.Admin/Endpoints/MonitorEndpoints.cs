using RuoYi.Admin.Authorization;
using RuoYi.Data.Models;
using RuoYi.Framework;
using RuoYi.Framework.Cache;
using RuoYi.Framework.Utils;
using RuoYi.System.Services;

namespace RuoYi.Admin.Endpoints;

public static class MonitorEndpoints
{
    private static readonly List<SysCache> Caches =
    [
        new SysCache(CacheConstants.LOGIN_TOKEN_KEY, "用户信息"),
        new SysCache(CacheConstants.SYS_CONFIG_KEY, "配置信息"),
        new SysCache(CacheConstants.SYS_DICT_KEY, "数据字典"),
        new SysCache(CacheConstants.CAPTCHA_CODE_KEY, "验证码"),
        new SysCache(CacheConstants.PWD_ERR_CNT_KEY, "密码错误次数")
    ];

    public static IEndpointRouteBuilder MapMonitorEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var cache = endpoints.MapGroup("/monitor/cache").RequireAuthorization();
        cache.MapGet("", GetCacheInfoAsync).RequirePermission("monitor:cache:list");
        cache.MapGet("/getNames", GetCacheNames).RequirePermission("monitor:cache:list");
        cache.MapGet("/getKeys/{cacheName}", GetCacheKeys).RequirePermission("monitor:cache:list");
        cache.MapGet("/getValue/{cacheName}/{cacheKey}", GetCacheValue).RequirePermission("monitor:cache:list");
        cache.MapDelete("/clearCacheName/{cacheName}", ClearCacheName).RequirePermission("monitor:cache:list");
        cache.MapDelete("/clearCacheKey/{cacheKey}", ClearCacheKey).RequirePermission("monitor:cache:list");
        cache.MapDelete("/clearCacheAll", ClearCacheAll).RequirePermission("monitor:cache:list");

        var server = endpoints.MapGroup("/monitor/server").RequireAuthorization();
        server.MapGet("", GetServerInfo).RequirePermission("monitor:server:list");

        var druid = endpoints.MapGroup("/monitor/druid").RequireAuthorization();
        druid.MapGet("", GetDruidInfo);

        return endpoints;
    }

    private static async Task<AjaxResult> GetCacheInfoAsync(ICache cache)
    {
        var info = await cache.GetDbInfoAsync();
        var commandStats = await cache.GetDbInfoAsync("commandstats");
        var dbSize = await cache.GetDbSize();

        var result = new Dictionary<string, object>
        {
            ["info"] = info,
            ["dbSize"] = dbSize
        };

        var pieList = new List<Dictionary<string, string>>();
        foreach (var key in commandStats.Keys)
        {
            var property = commandStats[key];
            pieList.Add(new Dictionary<string, string>
            {
                ["name"] = StringUtils.StripStart(key, "cmdstat_"),
                ["value"] = StringUtils.SubstringBetween(property, "calls=", ",usec")
            });
        }

        result["commandStats"] = pieList;
        return AjaxResult.Success(result);
    }

    private static AjaxResult GetCacheNames() => AjaxResult.Success(Caches);

    private static AjaxResult GetCacheKeys(string cacheName, ICache cache) =>
        AjaxResult.Success(cache.GetDbKeys(cacheName + "*"));

    private static AjaxResult GetCacheValue(string cacheName, string cacheKey, ICache cache)
    {
        var cacheValue = cache.GetString(cacheKey);
        return AjaxResult.Success(new SysCache(cacheName, cacheKey, cacheValue));
    }

    private static AjaxResult ClearCacheName(string cacheName, ICache cache)
    {
        cache.RemoveByPattern(cacheName + "*");
        return AjaxResult.Success();
    }

    private static AjaxResult ClearCacheKey(string cacheKey, ICache cache)
    {
        cache.Remove(cacheKey);
        return AjaxResult.Success();
    }

    private static AjaxResult ClearCacheAll(ICache cache)
    {
        cache.RemoveByPattern("*");
        return AjaxResult.Success();
    }

    private static AjaxResult GetServerInfo(ServerService service) =>
        AjaxResult.Success(service.GetServerInfo());

    private static AjaxResult GetDruidInfo() => AjaxResult.Success();
}
