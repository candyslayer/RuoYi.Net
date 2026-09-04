using RuoYi.Admin.Authorization;
using RuoYi.Common.Enums;
using RuoYi.Common.Utils;
using RuoYi.Data.Dtos;
using RuoYi.Framework;
using RuoYi.System.Services;

namespace RuoYi.Admin.Endpoints;

public static class ConfigEndpoints
{
    public static IEndpointRouteBuilder MapConfigEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/system/config").RequireAuthorization();
        group.MapGet("/list", GetListAsync).RequirePermission("system:config:list");
        group.MapGet("/{id:int}", GetAsync).RequirePermission("system:config:query");
        group.MapGet("/configKey/{configKey}", GetConfigKey);
        group.MapPost("", AddAsync).RequirePermission("system:config:add");
        group.MapPut("", EditAsync).RequirePermission("system:config:edit");
        group.MapDelete("/{configIds}", RemoveAsync).RequirePermission("system:config:remove");
        group.MapPost("/export", ExportAsync).RequirePermission("system:config:export");
        group.MapDelete("/refreshCache", RefreshCache);
        return endpoints;
    }

    private static async Task<SqlSugarPagedList<SysConfigDto>> GetListAsync(SysConfigDto dto, SysConfigService service)
        => await service.GetDtoPagedListAsync(dto);

    private static async Task<AjaxResult> GetAsync(int id, SysConfigService service)
        => AjaxResult.Success(await service.GetAsync(id));

    private static AjaxResult GetConfigKey(string configKey, SysConfigService service)
        => AjaxResult.Success(service.SelectConfigByKey(configKey));

    private static async Task<AjaxResult> AddAsync(SysConfigDto dto, SysConfigService service)
    {
        if (!service.CheckConfigKeyUnique(dto))
            return AjaxResult.Error("新增参数'" + dto.ConfigName + "'失败，参数键名已存在");
        await service.InsertConfigAsync(dto);
        return AjaxResult.Success();
    }

    private static async Task<AjaxResult> EditAsync(SysConfigDto dto, SysConfigService service)
    {
        if (!service.CheckConfigKeyUnique(dto))
            return AjaxResult.Error("修改参数'" + dto.ConfigName + "'失败，参数键名已存在");
        return AjaxResult.Success(await service.UpdateConfigAsync(dto));
    }

    private static async Task<AjaxResult> RemoveAsync(string configIds, SysConfigService service)
    {
        await service.DeleteConfigByIdsAsync(configIds.SplitToList<int>().ToArray());
        return AjaxResult.Success();
    }

    private static async Task ExportAsync(SysConfigDto dto, SysConfigService service, HttpContext context)
        => await ExcelUtils.ExportAsync(context.Response, await service.GetDtoListAsync(dto));

    private static AjaxResult RefreshCache(SysConfigService service)
    {
        service.ResetConfigCache();
        return AjaxResult.Success();
    }
}
