using Autofac;
using Spry.BuildingBlocks.EventBus.Abstractions;
using Spry.BuildingBlocks.EventBus;
using Spry.BuildingBlocks.EventBusRabbitMQ;
using Spry.AuthServer.SeedWork;
using RabbitMQ.Client;

namespace Spry.AuthServer.Infrastructure
{
    public static class EventBusConfiguration
    {
        public static IServiceCollection AddEventBusConfiguration(this IServiceCollection services, ConfigurationManager configuration)
        {
            var busSettings = configuration.GetSection(EventBusSettings.settings).Get<EventBusSettings>()!;

            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                var factory = new ConnectionFactory()
                {
                    HostName = configuration["EventBusConnection"]
                };

                if (!string.IsNullOrEmpty(busSettings.UserName))
                {
                    factory.UserName = configuration["EventBusUserName"];
                }

                if (!string.IsNullOrEmpty(busSettings.Password))
                {
                    factory.Password = configuration["EventBusPassword"];
                }

                var retryCount = 50000;
                if (!string.IsNullOrEmpty(busSettings.RetryCount))
                {
                    retryCount = int.Parse(configuration["EventBusRetryCount"]!);
                }

                return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            });

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            var subscriptionClientName = "SSO-Identity";

            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                var retryCount = 5;
                if (!string.IsNullOrEmpty(busSettings.RetryCount))
                    retryCount = int.Parse(busSettings.RetryCount);

                return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope,
                    eventBusSubcriptionsManager, subscriptionClientName, retryCount);
            });

            return services;
        }
    }
}
