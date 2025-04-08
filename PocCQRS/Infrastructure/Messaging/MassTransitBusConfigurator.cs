using MassTransit;
using PocCQRS.EntryPoint.Consumer;
using PocCQRS.Infrastructure.Settings;
using static PocCQRS.Infrastructure.Settings.RabbitMQ;

namespace PocCQRS.Infrastructure.Messaging;

public static class MassTransitBusConfigurator
{
    public static IServiceCollection AddMassTransitBus(this IServiceCollection services, IAppSettings appSettings)
    {
        var config = appSettings.RabbitMQSettings.Main;

        services.AddMassTransit(registrationConfigurator =>
        {
            foreach (var queue in config.Queues)
            {
                Type consumerType = GetConsumerTypeByQueueName(queue.Value.Name);

                registrationConfigurator.AddConsumer(consumerType);
            }

            registrationConfigurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(config.Host, config.VirtualHost, hostConfigurator =>
                {
                    hostConfigurator.Username(config.Username);
                    hostConfigurator.Password(config.Password);
                });

                foreach (var queue in config.Queues)
                {
                    Type consumerType = GetConsumerTypeByQueueName(queue.Value.Name);

                    cfg.ReceiveEndpoint(queue.Value.Name, e =>
                    {
                        e.SetQueueArgument("x-dead-letter-exchange", queue.Value.DLQ.Exchange);
                        e.SetQueueArgument("x-dead-letter-routing-key", queue.Value.DLQ.Queue);
                        e.ConfigureConsumer(context, consumerType);
                    });
                }
            });
        });

        services.AddMassTransitHostedService();
        return services;
    }

    private static Type GetConsumerTypeByQueueName(string queueName)
    {
        var eventType = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == queueName);

        if (eventType == null)
        {
            throw new InvalidOperationException($"Tipo de evento '{queueName}' não encontrado.");
        }

        return typeof(EventConsumer<>).MakeGenericType(eventType);
    }
}