
using eCommerce.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace eCommerce.SharedLibrary.DependencyInjection
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddSharedServices<TContext>
            (this IServiceCollection services,IConfiguration config , string FileName) where TContext : DbContext
        {
            // Add generic Db Context
            services.AddDbContext<TContext>(opt =>
            {
                opt.UseSqlServer(config.GetConnectionString("LocolDb"),
                    sqlserverOption => sqlserverOption.EnableRetryOnFailure()
                    );
            });

            // Configure Serilog logging
            var logDirectory = Path.GetDirectoryName(FileName);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.File(path: $"{FileName}-.text",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz [{Level:u3} {message:lj}{NewLine}{Exception}]}",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();
            // Add JWt Authentication Scheme
            JWTAythenticationScheme.AddJWTAythenticationScheme(services, config);
            return services;
        }

        public static IApplicationBuilder UseSharedPolicies ( this IApplicationBuilder app)
        {
            // use Global Exception 
            app.UseMiddleware<GlobalException>();
            // Register middle ware to block outside API calls
            //app.UseMiddleware<ListenToOnlyApiGateway>();
            
            return app;
        }
    }
}
