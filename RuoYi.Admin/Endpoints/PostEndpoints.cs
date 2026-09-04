using RuoYi.Common.Enums;
using RuoYi.Common.Utils;
using RuoYi.Data.Dtos;
using RuoYi.Framework.Exceptions;
using RuoYi.System.Services;

namespace RuoYi.Admin.Endpoints;

public static class PostEndpoints
{
    public static IEndpointRouteBuilder MapPostEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/system/post").RequireAuthorization();

        group.MapGet("/list", (SysPostDto dto, SysPostService service) =>
            service.GetDtoPagedListAsync(dto));

        group.MapGet("/{id:long}", async (long? id, SysPostService service) =>
            AjaxResult.Success(await service.GetDtoAsync(id)));

        group.MapPost("", AddAsync);
        group.MapPut("", EditAsync);
        group.MapDelete("/{ids}", RemoveAsync);
        group.MapPost("/export", ExportAsync);
        group.MapGet("/optionselect", OptionSelectAsync);

        return endpoints;
    }

    private static async Task<AjaxResult> AddAsync(SysPostDto post, SysPostService service)
    {
        if (!await service.CheckPostNameUniqueAsync(post))
            throw new ServiceException($"新增岗位'{post.PostName}'失败，岗位名称已存在");
        if (!await service.CheckPostCodeUniqueAsync(post))
            throw new ServiceException($"新增岗位'{post.PostName}'失败，岗位编码已存在");

        return AjaxResult.Success(await service.InsertAsync(post));
    }

    private static async Task<AjaxResult> EditAsync(SysPostDto post, SysPostService service)
    {
        if (!await service.CheckPostNameUniqueAsync(post))
            throw new ServiceException($"修改岗位'{post.PostName}'失败，岗位名称已存在");
        if (!await service.CheckPostCodeUniqueAsync(post))
            throw new ServiceException($"修改岗位'{post.PostName}'失败，岗位编码已存在");

        return AjaxResult.Success(await service.UpdateAsync(post));
    }

    private static async Task<AjaxResult> RemoveAsync(string ids, SysPostService service)
    {
        var values = ids.SplitToList<long>();
        return AjaxResult.Success(await service.DeleteAsync(values));
    }

    private static async Task ExportAsync(SysPostDto dto, HttpResponse response, SysPostService service)
    {
        var list = await service.GetDtoListAsync(dto);
        await ExcelUtils.ExportAsync(response, list);
    }

    private static async Task<AjaxResult> OptionSelectAsync(SysPostService service)
    {
        return AjaxResult.Success(await service.SelectPostAllAsync());
    }
}
