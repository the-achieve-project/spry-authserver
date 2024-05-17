using Spry.Identity.Services;
using StackExchange.Redis;

namespace Spry.Identity.Infrastructure
{
    public static class ApplicationServiceConfiguration
    {
        public static IServiceCollection AddApplicationServiceConfiguration(this IServiceCollection services, WebApplicationBuilder builder)
        {
            Lazy<ConnectionMultiplexer> _lazyRedis = new(() =>
            {
                var cacheConnection = builder.Configuration.GetConnectionString("SpryRedisStore");
                return ConnectionMultiplexer.Connect(cacheConnection!);
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>(_lazyRedis.Value);
            builder.Services.AddScoped<AccountService>();
            builder.Services.AddScoped<MessagingService>();
            builder.Services.AddHostedService<Workers.Seeder>();

            return services;
        }
    }
}
