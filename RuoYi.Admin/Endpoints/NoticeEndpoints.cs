using RuoYi.Admin.Authorization;
using RuoYi.Common.Utils;
using RuoYi.Data.Dtos;
using RuoYi.System.Services;

namespace RuoYi.Admin.Endpoints;

public static class NoticeEndpoints
{
    public static IEndpointRouteBuilder MapNoticeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/system/notice").RequireAuthorization();
        group.MapGet("/list", (SysNoticeDto dto, SysNoticeService service) => service.GetDtoPagedListAsync(dto))
            .RequirePermission("system:notice:list");
        group.MapGet("/{id:int}", async (int id, SysNoticeService service) => AjaxResult.Success(await service.GetDtoAsync(id)))
            .RequirePermission("system:notice:query");
        group.MapPost("", async (SysNoticeDto dto, SysNoticeService service) => AjaxResult.Success(await service.InsertAsync(dto)))
            .RequirePermission("system:notice:add");
        group.MapPut("", async (SysNoticeDto dto, SysNoticeService service) => AjaxResult.Success(await service.UpdateAsync(dto)))
            .RequirePermission("system:notice:edit");
        group.MapDelete("/{ids}", async (string ids, SysNoticeService service) => AjaxResult.Success(await service.DeleteAsync(ids.SplitToList<long>())))
            .RequirePermission("system:notice:remove");
        return endpoints;
    }
}
