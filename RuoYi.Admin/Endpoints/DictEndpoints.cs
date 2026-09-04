using RuoYi.Common.Enums;
using RuoYi.Common.Utils;
using RuoYi.Data.Dtos;
using RuoYi.Data.Entities;
using RuoYi.Framework;
using RuoYi.System.Services;

namespace RuoYi.Admin.Endpoints;

public static class DictEndpoints
{
    public static IEndpointRouteBuilder MapDictEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var type = endpoints.MapGroup("/system/dict/type").RequireAuthorization();
        type.MapGet("/list", TypeListAsync);
        type.MapGet("/{id:long}", TypeGetAsync);
        type.MapPost("", TypeAddAsync);
        type.MapPut("", TypeEditAsync);
        type.MapDelete("/{ids}", TypeRemoveAsync);
        type.MapDelete("/refreshCache", RefreshCache);
        type.MapGet("/optionselect", TypeOptionSelectAsync);
        type.MapPost("/export", TypeExportAsync);

        var data = endpoints.MapGroup("/system/dict/data").RequireAuthorization();
        data.MapGet("/list", DataListAsync);
        data.MapGet("/{dictCode:long}", DataGetAsync);
        data.MapGet("/type/{dictType}", DataByTypeAsync);
        data.MapPost("", DataAddAsync);
        data.MapPut("", DataEditAsync);
        data.MapDelete("/{dictCodes}", DataRemoveAsync);
        data.MapPost("/export", DataExportAsync);
        return endpoints;
    }

    private static Task<SqlSugarPagedList<SysDictTypeDto>> TypeListAsync(SysDictTypeDto dto, SysDictTypeService service)
        => service.GetDtoPagedListAsync(dto);
    private static async Task<AjaxResult> TypeGetAsync(long id, SysDictTypeService service)
        => AjaxResult.Success(await service.GetAsync(id));
    private static async Task<AjaxResult> TypeAddAsync(SysDictTypeDto dto, SysDictTypeService service)
    {
        if (!await service.CheckDictTypeUniqueAsync(dto)) return AjaxResult.Error($"新增字典'{dto.DictName}'失败，字典类型已存在");
        return AjaxResult.Success(await service.InsertDictTypeAsync(dto));
    }
    private static async Task<AjaxResult> TypeEditAsync(SysDictTypeDto dto, SysDictTypeService service)
    {
        if (!await service.CheckDictTypeUniqueAsync(dto)) return AjaxResult.Error($"修改字典'{dto.DictName}'失败，字典类型已存在");
        return AjaxResult.Success(await service.UpdateDictTypeAsync(dto));
    }
    private static async Task<AjaxResult> TypeRemoveAsync(string ids, SysDictTypeService service)
    {
        await service.DeleteDictTypeByIdsAsync(ids.SplitToList<long>().ToArray());
        return AjaxResult.Success();
    }
    private static AjaxResult RefreshCache(SysDictTypeService service)
    {
        service.ResetDictCache();
        return AjaxResult.Success();
    }
    private static async Task<AjaxResult> TypeOptionSelectAsync(SysDictTypeService service)
        => AjaxResult.Success(await service.SelectDictTypeAllAsync());
    private static async Task TypeExportAsync(SysDictTypeDto dto, HttpResponse response, SysDictTypeService service)
        => await ExcelUtils.ExportAsync(response, await service.GetDtoListAsync(dto));

    private static Task<SqlSugarPagedList<SysDictDataDto>> DataListAsync(SysDictDataDto dto, SysDictDataService service)
        => service.GetDtoPagedListAsync(dto);
    private static async Task<AjaxResult> DataGetAsync(long dictCode, SysDictDataService service)
        => AjaxResult.Success(await service.GetAsync(dictCode));
    private static async Task<AjaxResult> DataByTypeAsync(string dictType, SysDictTypeService service)
        => AjaxResult.Success(await service.SelectDictDataByTypeAsync(dictType) ?? new List<SysDictData>());
    private static async Task<AjaxResult> DataAddAsync(SysDictDataDto dto, SysDictDataService service)
        => AjaxResult.Success(await service.InsertDictDataAsync(dto));
    private static async Task<AjaxResult> DataEditAsync(SysDictDataDto dto, SysDictDataService service)
        => AjaxResult.Success(await service.UpdateDictDataAsync(dto));
    private static async Task<AjaxResult> DataRemoveAsync(string dictCodes, SysDictDataService service)
    {
        await service.DeleteDictDataByIdsAsync(dictCodes.SplitToList<long>().ToArray());
        return AjaxResult.Success();
    }
    private static async Task DataExportAsync(SysDictDataDto dto, HttpResponse response, SysDictDataService service)
        => await ExcelUtils.ExportAsync(response, await service.GetDtoListAsync(dto));
}
