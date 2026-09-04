using Microsoft.AspNetCore.Http;
using RuoYi.Common.Data;
using RuoYi.Common.Enums;
using RuoYi.Common.Utils;
using RuoYi.Data.Dtos;
using RuoYi.Data.Entities;
using RuoYi.System.Services;

namespace RuoYi.Admin.Endpoints;

/// <summary>
/// 用户管理 Minimal API 端点。
/// 仅负责 HTTP 层适配，业务规则继续由 System Services 承担。
/// </summary>
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/system/user")
            .RequireAuthorization();

        group.MapGet("/list", GetListAsync)
            .WithName("SystemUserList");

        group.MapGet("", GetInfoAsync)
            .WithName("SystemUserInfo");

        group.MapGet("/{userId:long}", GetInfoAsync)
            .WithName("SystemUserInfoById");

        group.MapPost("", AddAsync)
            .WithName("SystemUserAdd");

        group.MapPut("", EditAsync)
            .WithName("SystemUserEdit");

        group.MapDelete("/{userIds}", RemoveAsync)
            .WithName("SystemUserRemove");

        group.MapPut("/resetPwd", ResetPwdAsync)
            .WithName("SystemUserResetPwd");

        group.MapPut("/changeStatus", ChangeStatusAsync)
            .WithName("SystemUserChangeStatus");

        group.MapGet("/authRole/{userId:long}", GetAuthRoleAsync)
            .WithName("SystemUserAuthRole");

        group.MapPut("/authRole", InsertAuthRoleAsync)
            .WithName("SystemUserInsertAuthRole");

        group.MapGet("/deptTree", GetDeptTreeAsync)
            .WithName("SystemUserDeptTree");

        group.MapPost("/importData", ImportAsync)
            .WithName("SystemUserImport");

        group.MapPost("/importTemplate", DownloadImportTemplateAsync)
            .WithName("SystemUserImportTemplate");

        group.MapPost("/export", ExportAsync)
            .WithName("SystemUserExport");

        return endpoints;
    }

    private static Task<SqlSugarPagedList<SysUser>> GetListAsync(
        SysUserDto dto,
        SysUserService service)
        => service.GetPagedUserListAsync(dto);

    private static async Task<AjaxResult> GetInfoAsync(
        long? userId,
        SysUserService userService,
        SysRoleService roleService,
        SysPostService postService)
    {
        await userService.CheckUserDataScope(userId);
        var roles = await roleService.GetListAsync(new SysRoleDto());
        var posts = await postService.GetListAsync(new SysPostDto());

        var ajax = AjaxResult.Success();
        ajax.Add("roles", SecurityUtils.IsAdmin(userId)
            ? roles
            : roles.Where(r => !SecurityUtils.IsAdminRole(r.RoleId)));
        ajax.Add("posts", posts);

        if (userId.HasValue && userId > 0)
        {
            var user = await userService.GetDtoAsync(userId);
            ajax.Add(AjaxResult.DATA_TAG, user);
            ajax.Add("postIds", postService.GetPostIdsListByUserId(userId.Value));
            ajax.Add("roleIds", user.Roles.Select(x => x.RoleId).ToList());
        }

        return ajax;
    }

    private static async Task<AjaxResult> AddAsync(
        SysUserDto user,
        SysUserService service)
    {
        if (!await service.CheckUserNameUniqueAsync(user))
            return AjaxResult.Error($"新增用户'{user.UserName}'失败，登录账号已存在");

        if (!string.IsNullOrEmpty(user.Phonenumber) && !await service.CheckPhoneUniqueAsync(user))
            return AjaxResult.Error($"新增用户'{user.UserName}'失败，手机号码已存在");

        if (!string.IsNullOrEmpty(user.Email) && !await service.CheckEmailUniqueAsync(user))
            return AjaxResult.Error($"新增用户'{user.UserName}'失败，邮箱账号已存在");

        return AjaxResult.Success(service.InsertUser(user));
    }

    private static async Task<AjaxResult> EditAsync(
        SysUserDto user,
        SysUserService service)
    {
        service.CheckUserAllowed(user);
        await service.CheckUserDataScope(user.UserId ?? 0);

        if (!await service.CheckUserNameUniqueAsync(user))
            return AjaxResult.Error($"修改用户'{user.UserName}'失败，登录账号已存在");

        if (!string.IsNullOrEmpty(user.Phonenumber) && !await service.CheckPhoneUniqueAsync(user))
            return AjaxResult.Error($"修改用户'{user.UserName}'失败，手机号码已存在");

        if (!string.IsNullOrEmpty(user.Email) && !await service.CheckEmailUniqueAsync(user))
            return AjaxResult.Error($"修改用户'{user.UserName}'失败，邮箱账号已存在");

        return AjaxResult.Success(service.UpdateUser(user));
    }

    private static async Task<AjaxResult> RemoveAsync(
        string userIds,
        SysUserService service)
    {
        var ids = userIds.SplitToList<long>();
        if (ids.Contains(SecurityUtils.GetUserId()))
            return AjaxResult.Error("当前用户不能删除");

        return AjaxResult.Success(await service.DeleteUserByIdsAsync(ids));
    }

    private static async Task<AjaxResult> ResetPwdAsync(
        SysUserDto user,
        SysUserService service)
    {
        service.CheckUserAllowed(user);
        await service.CheckUserDataScope(user.UserId ?? 0);
        return AjaxResult.Success(service.ResetPwd(user));
    }

    private static async Task<AjaxResult> ChangeStatusAsync(
        SysUserDto user,
        SysUserService service)
    {
        service.CheckUserAllowed(user);
        await service.CheckUserDataScope(user.UserId ?? 0);
        return AjaxResult.Success(await service.UpdateUserStatus(user));
    }

    private static async Task<AjaxResult> GetAuthRoleAsync(
        long userId,
        SysUserService userService,
        SysRoleService roleService)
    {
        var user = await userService.GetDtoAsync(userId);
        var roles = await roleService.GetRolesByUserIdAsync(userId);

        var ajax = AjaxResult.Success();
        ajax.Add("user", user);
        ajax.Add("roles", SecurityUtils.IsAdmin(userId)
            ? roles
            : roles.Where(r => !SecurityUtils.IsAdminRole(r.RoleId)));
        return ajax;
    }

    private static async Task<AjaxResult> InsertAuthRoleAsync(
        long userId,
        string roleIds,
        SysUserService service)
    {
        var ids = roleIds.SplitToList<long>();
        await service.CheckUserDataScope(userId);
        service.InsertUserAuth(userId, ids);
        return AjaxResult.Success();
    }

    private static async Task<AjaxResult> GetDeptTreeAsync(
        SysDeptDto dept,
        SysDeptService service)
    {
        var data = await service.GetDeptTreeListAsync(dept);
        return AjaxResult.Success(data);
    }

    private static async Task<AjaxResult> ImportAsync(
        HttpRequest request,
        SysUserService service)
    {
        if (!request.HasFormContentType)
            return AjaxResult.Error("请求必须使用 multipart/form-data");

        var form = await request.ReadFormAsync();
        var file = form.Files.GetFile("file");
        if (file is null || file.Length == 0)
            return AjaxResult.Error("请选择要导入的文件");

        var updateSupport = bool.TryParse(form["updateSupport"].FirstOrDefault(), out var value) && value;

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        var list = await ExcelUtils.ImportAllAsync<SysUserDto>(stream);
        var msg = await service.ImportDtosAsync(list, updateSupport, SecurityUtils.GetUsername());
        return AjaxResult.Success(msg);
    }

    private static Task DownloadImportTemplateAsync(HttpResponse response)
        => ExcelUtils.GetImportTemplateAsync<SysUserDto>(response, "用户数据");

    private static async Task ExportAsync(
        SysUserDto dto,
        HttpResponse response,
        SysUserService service)
    {
        var list = await service.GetUserListAsync(dto);
        var dtos = service.ToDtos(list);
        await ExcelUtils.ExportAsync(response, dtos);
    }
}
