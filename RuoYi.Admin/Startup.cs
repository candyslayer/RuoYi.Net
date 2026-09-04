using Microsoft.AspNetCore.HttpOverrides;
using RuoYi.Framework.RateLimit;

namespace RuoYi.Admin
{
    [AppStartup(10000)]
    public class Startup : AppStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConsoleFormatter();
            services.AddCorsAccessor();

            // JWT 鉴权
            services.AddRyJwt();

            // 如果服务器端使用了 nginx/iis 等反向代理工具，可添加以下代码配置。
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            #region 日志
            services.AddMonitorLogging();

            Array.ForEach(new[] { LogLevel.Information, LogLevel.Warning, LogLevel.Error }, logLevel =>
            {
                services.AddFileLogging("logs/application-{1}-{0:yyyy}-{0:MM}-{0:dd}.log", options =>
                {
                    options.FileNameRule = fileName => string.Format(fileName, DateTime.UtcNow, logLevel.ToString());
                    options.WriteFilter = logMsg => logMsg.LogLevel == logLevel;
                });
            });
            #endregion

            // 远程请求
            services.AddRemoteRequest();

            // SqlSugar
            services.AddSqlSugarScope();

            // Cache
            services.AddCache();

            // SignalR
            services.AddSignalR();

            // captcha
            services.AddLazyCaptcha();

            // 自定义拦截器 (AspectCore)
            services.ConfigureDynamicProxy();

            // 限流
            services.AddConcurrencyLimiter();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // 如果服务器端使用了 nginx/iis 等反向代理工具，必须在管道前面处理转发头。
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Minimal API 不再依赖 /Home/Error 控制器路由。
                app.UseExceptionHandler();
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseRyStaticFiles(env);
            app.UseRouting();
            app.UseCorsAccessor();
            app.UseAuthentication();
            app.UseAuthorization();

            // 注入基础中间件
            app.UseInject();

            // 限流
            app.UseRateLimiter();
        }
    }
}
