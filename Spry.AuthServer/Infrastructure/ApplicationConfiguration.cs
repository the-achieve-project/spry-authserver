using Microsoft.EntityFrameworkCore;
using Quartz;
using Spry.AuthServer.Data;
using Spry.AuthServer.SeedWork;
using Spry.AuthServer.Services;
using StackExchange.Redis;

namespace Spry.AuthServer.Infrastructure
{
    public static class ApplicationConfiguration
    {
        public static IServiceCollection AddApplicationServiceConfiguration(this IServiceCollection services, WebApplicationBuilder builder)
        {
            Lazy<ConnectionMultiplexer> _lazyRedis = new(() =>
            {
                var cacheConnection = builder.Configuration.GetConnectionString("SpryRedisStore");
                return ConnectionMultiplexer.Connect(cacheConnection!);
            });
            builder.Services.Configure<IdentityServerSettings>(builder.Configuration.GetSection("IdentityServer"));

            builder.Services.AddQuartz(options =>
            {
                options.UseSimpleTypeLoader();
                options.UseInMemoryStore();
            });

            builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            builder.Services.AddSingleton<IConnectionMultiplexer>(_lazyRedis.Value);
            builder.Services.AddScoped<AccountService>();
            builder.Services.AddScoped<MessagingService>();
            builder.Services.AddHostedService<Workers.Seeder>();

            return services;
        }

        public static WebApplication UseMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetService<ILogger<Program>>();

            logger!.LogInformation("Migrating IdentityDataContext start");
            var db = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();

            db.Database.Migrate();
            logger!.LogInformation("Migrating IdentityDataContext end");

            return app;
        }
    }
}
