using RuoYi.Common.Utils;
using RuoYi.Data.Dtos;
using RuoYi.System.Services;

namespace RuoYi.Admin.Endpoints;

public static class NoticeEndpoints
{
    public static IEndpointRouteBuilder MapNoticeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/system/notice").RequireAuthorization();
        group.MapGet("/list", (SysNoticeDto dto, SysNoticeService service) => service.GetDtoPagedListAsync(dto));
        group.MapGet("/{id:int}", async (int id, SysNoticeService service) => AjaxResult.Success(await service.GetDtoAsync(id)));
        group.MapPost("", async (SysNoticeDto dto, SysNoticeService service) => AjaxResult.Success(await service.InsertAsync(dto)));
        group.MapPut("", async (SysNoticeDto dto, SysNoticeService service) => AjaxResult.Success(await service.UpdateAsync(dto)));
        group.MapDelete("/{ids}", async (string ids, SysNoticeService service) => AjaxResult.Success(await service.DeleteAsync(ids.SplitToList<long>())));
        return endpoints;
    }
}
