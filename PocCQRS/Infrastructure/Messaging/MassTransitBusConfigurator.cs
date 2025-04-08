using MassTransit;
using PocCQRS.EntryPoint.Consumer;
using PocCQRS.Infrastructure.Settings;

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
                        e.ConfigureConsumer(context, consumerType);
                        // Configuração do exchange/queue da DLQ
                        //e.SetQueueArgument("x-dead-letter-exchange", queue.Value.DLQ.Exchange);
                        //e.SetQueueArgument("x-dead-letter-routing-key", queue.Value.DLQ.Queue);
                        //e.SetQueueArgument("x-message-ttl", queue.Value.DLQ.TTL);

                    });

                    cfg.ReceiveEndpoint(queue.Value.DLQ.Queue, e =>
                    {
                        e.Durable = queue.Value.DLQ.Durable;
                        e.AutoDelete = queue.Value.DLQ.AutoDelete;
                        e.Bind(queue.Value.DLQ.Exchange);
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

        return typeof(EventConsumer<>).MakeGenericType(eventType);
    }
}