using RuoYi.Admin.Authorization;
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
        group.MapGet("/list", (SysPostDto dto, SysPostService service) => service.GetDtoPagedListAsync(dto))
            .RequirePermission("system:post:list");
        group.MapGet("/{id:long}", async (long? id, SysPostService service) => AjaxResult.Success(await service.GetDtoAsync(id)))
            .RequirePermission("system:post:query");
        group.MapPost("", AddAsync).RequirePermission("system:post:add");
        group.MapPut("", EditAsync).RequirePermission("system:post:edit");
        group.MapDelete("/{ids}", RemoveAsync).RequirePermission("system:post:remove");
        group.MapPost("/export", ExportAsync).RequirePermission("system:post:export");
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
        => AjaxResult.Success(await service.DeleteAsync(ids.SplitToList<long>()));

    private static async Task ExportAsync(SysPostDto dto, HttpResponse response, SysPostService service)
        => await ExcelUtils.ExportAsync(response, await service.GetDtoListAsync(dto));

    private static async Task<AjaxResult> OptionSelectAsync(SysPostService service)
        => AjaxResult.Success(await service.SelectPostAllAsync());
}
