using RuoYi.Common.Constants;
using RuoYi.Common.Enums;
using RuoYi.Common.Utils;
using RuoYi.System.Services;

namespace RuoYi.Admin.Endpoints;

public static class MenuEndpoints
{
    public static IEndpointRouteBuilder MapMenuEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/system/menu").RequireAuthorization();
        group.MapGet("/list", ListAsync);
        group.MapGet("/{menuId:long}", GetAsync);
        group.MapGet("/treeselect", TreeSelectAsync);
        group.MapGet("/roleMenuTreeselect/{roleId:long}", RoleMenuTreeSelectAsync);
        group.MapPost("", AddAsync);
        group.MapPut("", EditAsync);
        group.MapDelete("/{menuId:long}", RemoveAsync);
        return endpoints;
    }

    private static async Task<AjaxResult> ListAsync(SysMenuDto dto, SysMenuService service)
        => AjaxResult.Success(await service.SelectMenuListAsync(dto, SecurityUtils.GetUserId()));

    private static async Task<AjaxResult> GetAsync(long menuId, SysMenuService service)
        => AjaxResult.Success(await service.GetAsync(menuId));

    private static async Task<AjaxResult> TreeSelectAsync(SysMenuDto dto, SysMenuService service)
        => AjaxResult.Success(service.BuildMenuTreeSelect(await service.SelectMenuListAsync(dto, SecurityUtils.GetUserId())));

    private static async Task<AjaxResult> RoleMenuTreeSelectAsync(long roleId, SysMenuService service)
    {
        var menus = await service.SelectMenuListAsync(SecurityUtils.GetUserId());
        var result = AjaxResult.Success();
        result.Add("checkedKeys", service.SelectMenuListByRoleId(roleId));
        result.Add("menus", service.BuildMenuTreeSelect(menus));
        return result;
    }

    private static async Task<AjaxResult> AddAsync(SysMenuDto menu, SysMenuService service)
    {
        if (!service.CheckMenuNameUnique(menu))
            return AjaxResult.Error($"新增菜单'{menu.MenuName}'失败，菜单名称已存在");
        if (UserConstants.YES_FRAME.Equals(menu.IsFrame) && !StringUtils.IsHttp(menu.Path))
            return AjaxResult.Error($"新增菜单'{menu.MenuName}'失败，地址必须以http(s)://开头");
        return AjaxResult.Success(await service.InsertAsync(menu));
    }

    private static async Task<AjaxResult> EditAsync(SysMenuDto menu, SysMenuService service)
    {
        if (!service.CheckMenuNameUnique(menu))
            return AjaxResult.Error($"修改菜单'{menu.MenuName}'失败，菜单名称已存在");
        if (UserConstants.YES_FRAME.Equals(menu.IsFrame) && !StringUtils.IsHttp(menu.Path))
            return AjaxResult.Error($"修改菜单'{menu.MenuName}'失败，地址必须以http(s)://开头");
        if (menu.MenuId.Equals(menu.ParentId))
            return AjaxResult.Error($"修改菜单'{menu.MenuName}'失败，上级菜单不能选择自己");
        return AjaxResult.Success(await service.UpdateAsync(menu));
    }

    private static async Task<AjaxResult> RemoveAsync(long menuId, SysMenuService service)
    {
        if (service.HasChildByMenuId(menuId))
            return AjaxResult.Error("存在子菜单,不允许删除");
        if (service.CheckMenuExistRole(menuId))
            return AjaxResult.Error("菜单已分配,不允许删除");
        return AjaxResult.Success(await service.DeleteAsync(menuId));
    }
}
