using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PocCQRS.Infrastructure.Messaging;
using PocCQRS.Infrastructure.Settings;

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
                        e.ConfigureConsumer(context, consumerType);
                    });
                }
            });
        });
        
        services.AddMassTransitHostedService();
        return services;
    }
    
    static Type GetConsumerTypeByQueueName(string queueName)
    {
        var eventType = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == queueName);
    
        if (eventType == null)
        {
            throw new InvalidOperationException($"Tipo de evento '{queueName}' não encontrado.");
        }

        return typeof(Consumer<>).MakeGenericType(eventType);
    }
}