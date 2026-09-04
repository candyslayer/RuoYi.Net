using RuoYi.Common.Enums;
using RuoYi.Common.Utils;
using RuoYi.Data.Dtos;
using RuoYi.Data.Models;
using RuoYi.Framework.Cache;
using RuoYi.System.Services;
using SqlSugar;

namespace RuoYi.Admin.Endpoints;

public static class LogEndpoints
{
    public static IEndpointRouteBuilder MapLogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var login = endpoints.MapGroup("/monitor/logininfor").RequireAuthorization();
        login.MapGet("/list", GetLoginListAsync).RequirePermission("system:logininfor:list");
        login.MapGet("/{id:long}", GetLoginAsync).RequirePermission("system:logininfor:query");
        login.MapGet("", GetLoginAsync).RequirePermission("system:logininfor:query");
        login.MapPost("", AddLoginAsync).RequirePermission("system:logininfor:add");
        login.MapPut("", EditLoginAsync).RequirePermission("system:logininfor:edit");
        login.MapDelete("/{ids}", RemoveLoginAsync).RequirePermission("system:logininfor:remove");
        login.MapPost("/import", ImportLoginAsync).RequirePermission("system:logininfor:import");
        login.MapPost("/export", ExportLoginAsync).RequirePermission("system:logininfor:export");

        var oper = endpoints.MapGroup("/monitor/operlog").RequireAuthorization();
        oper.MapGet("/list", GetOperListAsync).RequirePermission("system:operlog:list");
        oper.MapDelete("/{ids}", RemoveOperAsync).RequirePermission("system:operlog:remove");
        oper.MapDelete("/clean", CleanOper).RequirePermission("system:operlog:remove");
        oper.MapPost("/export", ExportOperAsync).RequirePermission("system:operlog:export");

        var online = endpoints.MapGroup("/monitor/online").RequireAuthorization();
        online.MapGet("/list", GetOnlineListAsync).RequirePermission("monitor:online:list");
        online.MapDelete("/{tokenId}", ForceLogout).RequirePermission("monitor:online:forceLogout");

        return endpoints;
    }

    private static Task<SqlSugarPagedList<SysLogininforDto>> GetLoginListAsync(SysLogininforDto dto, SysLogininforService service) =>
        service.GetDtoPagedListAsync(dto);

    private static async Task<AjaxResult> GetLoginAsync(long id, SysLogininforService service) =>
        AjaxResult.Success(await service.GetDtoAsync(id));

    private static async Task<AjaxResult> AddLoginAsync(SysLogininforDto dto, SysLogininforService service) =>
        AjaxResult.Success(await service.InsertAsync(dto));

    private static async Task<AjaxResult> EditLoginAsync(SysLogininforDto dto, SysLogininforService service) =>
        AjaxResult.Success(await service.UpdateAsync(dto));

    private static async Task<AjaxResult> RemoveLoginAsync(string ids, SysLogininforService service) =>
        AjaxResult.Success(await service.DeleteAsync(ids.SplitToList<long>().ToArray()));

    private static async Task<IResult> ImportLoginAsync(HttpRequest request, SysLogininforService service)
    {
        if (!request.HasFormContentType)
            return Results.BadRequest(AjaxResult.Error("请求必须使用 multipart/form-data"));

        var form = await request.ReadFormAsync();
        var file = form.Files.GetFile("file");
        if (file == null)
            return Results.BadRequest(AjaxResult.Error("文件不能为空"));

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;
        var list = await ExcelUtils.ImportAsync<SysLogininforDto>(stream);
        await service.ImportDtoBatchAsync(list);
        return Results.Ok(AjaxResult.Success());
    }

    private static async Task ExportLoginAsync(SysLogininforDto dto, SysLogininforService service, HttpContext context)
    {
        var list = await service.GetDtoListAsync(dto);
        await ExcelUtils.ExportAsync(context.Response, list);
    }

    private static Task<SqlSugarPagedList<SysOperLogDto>> GetOperListAsync(SysOperLogDto dto, SysOperLogService service) =>
        service.GetDtoPagedListAsync(dto);

    private static async Task<AjaxResult> RemoveOperAsync(string ids, SysOperLogService service) =>
        AjaxResult.Success(await service.DeleteAsync(ids.SplitToList<long>().ToArray()));

    private static AjaxResult CleanOper(SysOperLogService service)
    {
        service.Clean();
        return AjaxResult.Success();
    }

    private static async Task ExportOperAsync(SysOperLogDto dto, SysOperLogService service, HttpContext context)
    {
        var list = await service.GetDtoListAsync(dto);
        await ExcelUtils.ExportAsync(context.Response, list);
    }

    private static async Task<SqlSugarPagedList<SysUserOnline>> GetOnlineListAsync(
        string? ipaddr,
        string? userName,
        ICache cache,
        SysUserOnlineService service)
    {
        var keys = cache.GetDbKeys(CacheConstants.LOGIN_TOKEN_KEY + "*", 10000);
        var list = new List<SysUserOnline>();

        foreach (var key in keys)
        {
            LoginUser user = await cache.GetAsync<LoginUser>(key);
            if (StringUtils.IsNotEmpty(ipaddr) && StringUtils.IsNotEmpty(userName))
                list.Add(service.GetOnlineByInfo(ipaddr, userName, user));
            else if (StringUtils.IsNotEmpty(ipaddr))
                list.Add(service.GetOnlineByIpaddr(ipaddr, user));
            else if (StringUtils.IsNotEmpty(userName) && user.User != null)
                list.Add(service.GetOnlineByUserName(userName, user));
            else
                list.Add(service.LoginUserToUserOnline(user));
        }

        list = list.Where(u => u != null).ToList();
        return new SqlSugarPagedList<SysUserOnline> { Rows = list, Total = list.Count };
    }

    private static AjaxResult ForceLogout(string tokenId, ICache cache)
    {
        cache.Remove(CacheConstants.LOGIN_TOKEN_KEY + tokenId);
        return AjaxResult.Success();
    }
}
