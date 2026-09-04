using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using RuoYi.Common.Enums;
using RuoYi.Common.Utils;
using RuoYi.Framework.Authorization;
using RuoYi.Quartz.Dtos;
using RuoYi.Quartz.Services;
using RuoYi.System;

namespace RuoYi.Quartz.Endpoints;

public static class QuartzEndpoints
{
    public static void MapQuartzEndpoints(this IEndpointRouteBuilder app)
    {
        var job = app.MapGroup("/monitor/job").RequireAuthorization();

        job.MapGet("/list", async ([AsParameters] SysJobDto dto, SysJobService service) =>
            await service.GetDtoPagedListAsync(dto))
            .RequirePermission("monitor:job:list");

        job.MapGet("", async (long jobId, SysJobService service) =>
            AjaxResult.Success(await service.GetDtoAsync(jobId)))
            .RequirePermission("monitor:job:query");

        job.MapGet("/{jobId:long}", async (long jobId, SysJobService service) =>
            AjaxResult.Success(await service.GetDtoAsync(jobId)))
            .RequirePermission("monitor:job:query");

        job.MapPost("", async (SysJobDto dto, SysJobService service) =>
        {
            var msg = service.CheckJob(dto);
            if (!string.IsNullOrEmpty(msg)) return AjaxResult.Error($"新增任务{msg}");
            return await service.InsertJobAsync(dto) ? AjaxResult.Success() : AjaxResult.Error();
        }).RequirePermission("monitor:job:add");

        job.MapPut("", async (SysJobDto dto, SysJobService service) =>
        {
            var msg = service.CheckJob(dto);
            if (!string.IsNullOrEmpty(msg)) return AjaxResult.Error($"新增任务{msg}");
            return await service.UpdateJobAsync(dto) ? AjaxResult.Success() : AjaxResult.Error();
        }).RequirePermission("monitor:job:edit");

        job.MapPut("/changeStatus", async (SysJobDto dto, SysJobService service) =>
            await service.ChangeStatusAsync(dto) ? AjaxResult.Success() : AjaxResult.Error())
            .RequirePermission("monitor:job:edit");

        job.MapPut("/run", async (SysJobDto dto, SysJobService service) =>
            await service.Run(dto) ? AjaxResult.Success() : AjaxResult.Error("任务不存在或已过期！"))
            .RequirePermission("monitor:job:changeStatus");

        job.MapDelete("/{jobIds}", async (string jobIds, SysJobService service) =>
        {
            var ids = jobIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(long.Parse).ToArray();
            return AjaxResult.Success(await service.DeleteAsync(ids));
        }).RequirePermission("monitor:job:remove");

        job.MapPost("/export", async (SysJobDto dto, SysJobService service) =>
        {
            var dtos = await service.GetDtoListAsync(dto);
            await ExcelUtils.ExportAsync(App.HttpContext.Response, dtos, sheetName: "定时任务");
        }).RequirePermission("monitor:job:export");

        var log = app.MapGroup("/monitor/jobLog").RequireAuthorization();

        log.MapGet("/list", async ([AsParameters] SysJobLogDto dto, SysJobLogService service) =>
            await service.GetDtoPagedListAsync(dto))
            .RequirePermission("monitor:job:list");

        log.MapGet("", async (long jobLogId, SysJobLogService service) =>
            AjaxResult.Success(await service.GetDtoAsync(jobLogId)))
            .RequirePermission("monitor:job:query");

        log.MapGet("/{jobLogId:long}", async (long jobLogId, SysJobLogService service) =>
            AjaxResult.Success(await service.GetDtoAsync(jobLogId)))
            .RequirePermission("monitor:job:query");

        log.MapDelete("/{jobLogIds}", async (string jobLogIds, SysJobLogService service) =>
        {
            var ids = jobLogIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(long.Parse).ToArray();
            return AjaxResult.Success(await service.DeleteAsync(ids));
        }).RequirePermission("monitor:job:remove");

        log.MapDelete("/clean", (SysJobLogService service) =>
        {
            service.Clean();
            return AjaxResult.Success();
        }).RequirePermission("monitor:job:remove");

        log.MapPost("/export", async (SysJobLogDto dto, SysJobLogService service) =>
        {
            var dtos = await service.GetDtoListAsync(dto);
            await ExcelUtils.ExportAsync(App.HttpContext.Response, dtos, sheetName: "调度日志");
        }).RequirePermission("monitor:job:export");
    }
}
