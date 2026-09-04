<p align="center">
	<img alt="logo" src="https://oscimg.oschina.net/oscnet/up-dd77653d7c9f197dd9d93684f3c8dcfbab6.png">
</p>
<h1 align="center" style="margin: 30px 0 30px; font-weight: bold;">RuoYi.Net Minimal API</h1>
<h4 align="center">基于 .NET 8 Minimal API 改造的若依管理框架</h4>

## 项目说明

本仓库基于 `wdyday/RuoYi.Net` 改造，目标是将 HTTP API 层逐步迁移到 ASP.NET Core Minimal API，同时保留原有业务服务、权限体系、缓存、日志、定时任务等能力。

当前采用增量迁移方式：新的接口放在 `RuoYi.Admin/Endpoints` 下，旧 MVC Controller 在迁移完成前暂时保留，以保证业务模块可以逐步切换。

## 技术框架及依赖

- .NET 8
- ASP.NET Core Minimal API
- SqlSugar
- JWT
- AspectCore
- SignalR
- Vue2 + Element UI

## 当前改造

1. Admin 启动入口改为 .NET 8 top-level statements。
2. 新增 Minimal API Endpoint 组织方式。
3. 登录、退出、当前用户信息、路由信息已迁移到 Minimal API。
4. `RuoYi.Generator` 已从解决方案和 Admin 项目依赖中移除。
5. 后续系统管理、监控、定时任务等 Controller 将继续按模块迁移。

## 原有功能

1. 用户管理
2. 部门管理
3. 岗位管理
4. 菜单管理
5. 角色管理
6. 字典管理
7. 参数管理
8. 通知公告
9. 操作日志
10. 登录日志
11. 在线用户
12. 定时任务
13. 系统接口
14. 服务监控
15. 缓存监控
16. 在线构建器

## 说明

这是针对原 RuoYi.Net 的结构化重构版本，不再保留代码生成模块。
