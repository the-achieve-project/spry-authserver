﻿using Autofac;
using Spry.BuildingBlocks.EventBus.Abstractions;
using Spry.BuildingBlocks.EventBus;
using Spry.BuildingBlocks.EventBusRabbitMQ;
using Spry.Identity.SeedWork;
using RabbitMQ.Client;

namespace Spry.Identity.Infrastructure
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
                    HostName = busSettings.Connection
                };

                if (!string.IsNullOrEmpty(busSettings.UserName))
                {
                    factory.UserName = busSettings.UserName;
                }

                if (!string.IsNullOrEmpty(busSettings.Password))
                {
                    factory.Password = busSettings.Password;
                }

                var retryCount = 50000;
                if (!string.IsNullOrEmpty(busSettings.RetryCount))
                {
                    retryCount = int.Parse(busSettings.RetryCount);
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
