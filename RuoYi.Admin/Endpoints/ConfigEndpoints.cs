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

        group.MapGet("/list", GetListAsync);
        group.MapGet("/{id:int}", GetAsync);
        group.MapGet("/configKey/{configKey}", GetConfigKey);
        group.MapPost("", AddAsync);
        group.MapPut("", EditAsync);
        group.MapDelete("/{configIds}", RemoveAsync);
        group.MapPost("/export", ExportAsync);
        group.MapDelete("/refreshCache", RefreshCache);

        return endpoints;
    }

    private static async Task<SqlSugarPagedList<SysConfigDto>> GetListAsync(
        SysConfigDto dto,
        SysConfigService service)
    {
        return await service.GetDtoPagedListAsync(dto);
    }

    private static async Task<AjaxResult> GetAsync(int id, SysConfigService service)
    {
        var data = await service.GetAsync(id);
        return AjaxResult.Success(data);
    }

    private static AjaxResult GetConfigKey(string configKey, SysConfigService service)
    {
        return AjaxResult.Success(service.SelectConfigByKey(configKey));
    }

    private static async Task<AjaxResult> AddAsync(SysConfigDto dto, SysConfigService service)
    {
        if (!service.CheckConfigKeyUnique(dto))
        {
            return AjaxResult.Error("新增参数'" + dto.ConfigName + "'失败，参数键名已存在");
        }

        await service.InsertConfigAsync(dto);
        return AjaxResult.Success();
    }

    private static async Task<AjaxResult> EditAsync(SysConfigDto dto, SysConfigService service)
    {
        if (!service.CheckConfigKeyUnique(dto))
        {
            return AjaxResult.Error("修改参数'" + dto.ConfigName + "'失败，参数键名已存在");
        }

        var data = await service.UpdateConfigAsync(dto);
        return AjaxResult.Success(data);
    }

    private static async Task<AjaxResult> RemoveAsync(string configIds, SysConfigService service)
    {
        var ids = configIds.SplitToList<int>().ToArray();
        await service.DeleteConfigByIdsAsync(ids);
        return AjaxResult.Success();
    }

    private static async Task ExportAsync(SysConfigDto dto, SysConfigService service, HttpContext context)
    {
        var list = await service.GetDtoListAsync(dto);
        await ExcelUtils.ExportAsync(context.Response, list);
    }

    private static AjaxResult RefreshCache(SysConfigService service)
    {
        service.ResetConfigCache();
        return AjaxResult.Success();
    }
}
