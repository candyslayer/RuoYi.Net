using RuoYi.Common.Enums;
using RuoYi.Common.Utils;
using RuoYi.Data.Models;
using RuoYi.System.Services;

namespace RuoYi.Admin.Endpoints;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/system/role").RequireAuthorization();

        group.MapGet("/list", GetListAsync);
        group.MapGet("/{id:long}", GetAsync);
        group.MapPost("", AddAsync);
        group.MapPut("", EditAsync);
        group.MapPut("/dataScope", SaveDataScopeAsync);
        group.MapPut("/changeStatus", ChangeStatusAsync);
        group.MapDelete("/{ids}", RemoveAsync);
        group.MapPost("/export", ExportAsync);
        group.MapPost("/optionselect", OptionSelectAsync);
        group.MapGet("/authUser/allocatedList", GetAllocatedListAsync);
        group.MapGet("/authUser/unallocatedList", GetUnallocatedListAsync);
        group.MapPut("/authUser/cancel", CancelAuthUserAsync);
        group.MapPut("/authUser/cancelAll", CancelAuthUserBatchAsync);
        group.MapPut("/authUser/selectAll", SaveAuthUserAllAsync);
        group.MapGet("/deptTree/{roleId:long}", GetDeptTreeAsync);

        return endpoints;
    }

    private static Task<SqlSugarPagedList<SysRoleDto>> GetListAsync(SysRoleDto dto, SysRoleService service)
        => service.GetPagedRoleListAsync(dto);

    private static async Task<AjaxResult> GetAsync(long id, SysRoleService service)
    {
        await service.CheckRoleDataScopeAsync(id);
        return AjaxResult.Success(await service.GetDtoAsync(id));
    }

    private static async Task<AjaxResult> AddAsync(SysRoleDto role, SysRoleService service)
    {
        if (!await service.CheckRoleNameUniqueAsync(role))
            return AjaxResult.Error($"新增角色'{role.RoleName}'失败，角色名称已存在");
        if (!await service.CheckRoleKeyUniqueAsync(role))
            return AjaxResult.Error($"新增角色'{role.RoleName}'失败，角色权限已存在");
        return AjaxResult.Success(await service.InsertRoleAsync(role));
    }

    private static async Task<AjaxResult> EditAsync(SysRoleDto role, SysRoleService service,
        SysPermissionService permissionService, SysUserService userService, TokenService tokenService)
    {
        service.CheckRoleAllowed(role);
        await service.CheckRoleDataScopeAsync(role.RoleId);
        if (!await service.CheckRoleNameUniqueAsync(role))
            return AjaxResult.Error($"修改角色'{role.RoleName}'失败，角色名称已存在");
        if (!await service.CheckRoleKeyUniqueAsync(role))
            return AjaxResult.Error($"修改角色'{role.RoleName}'失败，角色权限已存在");

        if (await service.UpdateRoleAsync(role) <= 0)
            return AjaxResult.Error($"修改角色'{role.RoleName}'失败，请联系管理员");

        var loginUser = SecurityUtils.GetLoginUser();
        if (loginUser.User != null && !SecurityUtils.IsAdmin(loginUser.User))
        {
            loginUser.Permissions = permissionService.GetMenuPermission(loginUser.User);
            loginUser.User = await userService.GetDtoByUsernameAsync(loginUser.User.UserName!);
            tokenService.SetLoginUser(loginUser);
        }
        return AjaxResult.Success();
    }

    private static async Task<AjaxResult> SaveDataScopeAsync(SysRoleDto role, SysRoleService service)
    {
        service.CheckRoleAllowed(role);
        await service.CheckRoleDataScopeAsync(role.RoleId);
        return AjaxResult.Success(await service.AuthDataScopeAsync(role));
    }

    private static async Task<AjaxResult> ChangeStatusAsync(SysRoleDto role, SysRoleService service)
    {
        service.CheckRoleAllowed(role);
        await service.CheckRoleDataScopeAsync(role.RoleId);
        return AjaxResult.Success(await service.UpdateRoleStatusAsync(role));
    }

    private static async Task<AjaxResult> RemoveAsync(string ids, SysRoleService service)
        => AjaxResult.Success(await service.DeleteRoleByIdsAsync(ids.SplitToList<long>()));

    private static async Task ExportAsync(SysRoleDto dto, HttpResponse response, SysRoleService service)
        => await ExcelUtils.ExportAsync(response, await service.GetRoleListAsync(dto));

    private static async Task<AjaxResult> OptionSelectAsync(SysRoleService service)
        => AjaxResult.Success(await service.GetListAsync(new SysRoleDto()));

    private static Task<SqlSugarPagedList<SysUserDto>> GetAllocatedListAsync(SysUserDto dto, SysUserService service)
        => service.GetPagedAllocatedListAsync(dto);

    private static Task<SqlSugarPagedList<SysUserDto>> GetUnallocatedListAsync(SysUserDto dto, SysUserService service)
        => service.GetPagedUnallocatedListAsync(dto);

    private static async Task<AjaxResult> CancelAuthUserAsync(SysUserRoleDto dto, SysRoleService service)
        => AjaxResult.Success(await service.DeleteAuthUserAsync(dto));

    private static async Task<AjaxResult> CancelAuthUserBatchAsync(SysUserRoleDto dto, SysRoleService service)
        => AjaxResult.Success(await service.DeleteAuthUserBathAsync(dto));

    private static async Task<AjaxResult> SaveAuthUserAllAsync(SysUserRoleDto dto, SysRoleService service)
    {
        await service.CheckRoleDataScopeAsync(dto.RoleId);
        return AjaxResult.Success(await service.InsertAuthUsersAsync(dto.RoleId, dto.UserIds));
    }

    private static async Task<AjaxResult> GetDeptTreeAsync(long roleId, SysDeptService deptService, SysMenuService menuService)
    {
        var ajax = AjaxResult.Success();
        ajax.Add("checkedKeys", await deptService.GetDeptListByRoleIdAsync(roleId));
        ajax.Add("depts", await deptService.GetDeptTreeListAsync(new SysDeptDto()));
        return ajax;
    }
}
