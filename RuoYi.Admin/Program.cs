using AspectCore.Extensions.DependencyInjection;
using RuoYi.Admin.Endpoints;

var builder = WebApplication.CreateBuilder(args).Inject();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(3);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
});

// 用 AspectCore 替换默认 IOC 容器，用于 AOP 拦截。
builder.Host.UseServiceProviderFactory(new DynamicProxyServiceProviderFactory());

var app = builder.Build();

// Minimal API endpoints.
app.MapSystemEndpoints();
app.MapUserEndpoints();
app.MapPostEndpoints();
app.MapNoticeEndpoints();
app.MapRoleEndpoints();
app.MapMenuEndpoints();
app.MapDeptEndpoints();
app.MapDictEndpoints();
app.MapConfigEndpoints();
app.MapProfileEndpoints();

app.Run();
