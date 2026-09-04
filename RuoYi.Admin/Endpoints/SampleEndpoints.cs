using Microsoft.AspNetCore.RateLimiting;
using RuoYi.Admin.Services;
using RuoYi.Data.Slave.Dtos;
using RuoYi.Framework.RateLimit;

namespace RuoYi.Admin.Endpoints;

public static class SampleEndpoints
{
    public static IEndpointRouteBuilder MapSampleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/Sample");

        group.MapGet("/{id:long?}", GetAsync);
        group.MapGet("/getWithPerminAndRole/{id:long?}", GetWithPerminAndRoleAsync)
            .RequirePermission("system:dept:query")
            .RequireRole("admin");
        group.MapGet("/rateLimit", RateLimit)
            .RequireRateLimiting(LimitType.Default);
        group.MapGet("/ipRateLimit", IpRateLimit)
            .RequireRateLimiting(LimitType.IP);
        group.MapPost("/updateUserBySql", UpdateUserBySqlAsync)
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<SlaveSysUserDto> GetAsync(
        long? id,
        RuoYi.System.Slave.Services.SysUserService slaveSysUserService)
    {
        return await slaveSysUserService.GetAsync(id);
    }

    private static async Task<SlaveSysUserDto> GetWithPerminAndRoleAsync(
        long? id,
        RuoYi.System.Slave.Services.SysUserService slaveSysUserService)
    {
        return await slaveSysUserService.GetAsync(id);
    }

    private static string RateLimit() => "rateLimit";

    private static string IpRateLimit() => "ipRateLimit";

    private static async Task<int> UpdateUserBySqlAsync(SampleService sampleService)
    {
        return await sampleService.UpdateUserAsync();
    }
}
