using RuoYi.Common.Constants;
using RuoYi.Common.Enums;
using RuoYi.System.Services;

namespace RuoYi.Admin.Endpoints;

public static class DeptEndpoints
{
    public static IEndpointRouteBuilder MapDeptEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/system/dept").RequireAuthorization();
        group.MapGet("/list", ListAsync);
        group.MapGet("/list/exclude/{deptId:long}", ExcludeChildListAsync);
        group.MapGet("/{deptId:long}", GetAsync);
        group.MapPost("", AddAsync);
        group.MapPut("", EditAsync);
        group.MapDelete("/{deptId:long}", RemoveAsync);
        return endpoints;
    }

    private static async Task<AjaxResult> ListAsync(SysDeptDto dto, SysDeptService service)
        => AjaxResult.Success(await service.GetDtoListAsync(dto));

    private static async Task<AjaxResult> ExcludeChildListAsync(long deptId, SysDeptService service)
    {
        var list = await service.GetDtoListAsync(new SysDeptDto());
        var data = list.Where(d => d.DeptId != deptId || (!d.Ancestors?.Split(',').Contains(deptId.ToString()) ?? false)).ToList();
        return AjaxResult.Success(data);
    }

    private static async Task<AjaxResult> GetAsync(long deptId, SysDeptService service)
    {
        await service.CheckDeptDataScopeAsync(deptId);
        return AjaxResult.Success(await service.GetDtoAsync(deptId));
    }

    private static async Task<AjaxResult> AddAsync(SysDeptDto dept, SysDeptService service)
    {
        if (!await service.CheckDeptNameUniqueAsync(dept))
            return AjaxResult.Error($"新增部门'{dept.DeptName} '失败，部门名称已存在");
        return AjaxResult.Success(await service.InsertDeptAsync(dept));
    }

    private static async Task<AjaxResult> EditAsync(SysDeptDto dept, SysDeptService service)
    {
        var deptId = dept.DeptId!.Value;
        await service.CheckDeptDataScopeAsync(deptId);
        if (!await service.CheckDeptNameUniqueAsync(dept))
            return AjaxResult.Error($"修改部门'{dept.DeptName}'失败，部门名称已存在");
        if (dept.ParentId.Equals(deptId))
            return AjaxResult.Error($"修改部门'{dept.DeptName}'失败，上级部门不能是自己");
        if (UserConstants.DEPT_DISABLE.Equals(dept.Status) && await service.CountNormalChildrenDeptByIdAsync(deptId) > 0)
            return AjaxResult.Error("该部门包含未停用的子部门！");
        return AjaxResult.Success(await service.UpdateDeptAsync(dept));
    }

    private static async Task<AjaxResult> RemoveAsync(long deptId, SysDeptService service)
    {
        if (await service.HasChildByDeptIdAsync(deptId))
            return AjaxResult.Error("存在下级部门,不允许删除");
        if (await service.CheckDeptExistUserAsync(deptId))
            return AjaxResult.Error("部门存在用户,不允许删除");
        await service.CheckDeptDataScopeAsync(deptId);
        return AjaxResult.Success(await service.DeleteDeptByIdAsync(deptId));
    }
}
