using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PocCQRS.Infrastructure.Messaging;
using PocCQRS.Infrastructure.Settings;

public static class MassTransitBusConfigurator
{
    public static IServiceCollection AddMassTransitBus(this IServiceCollection services, IAppSettings appSettings)
    {
        services.AddMassTransit(x =>
        {
            var settings = appSettings.RabbitMQSettings.Main;
        
            // Register the generic consumer type
            x.AddConsumer(typeof(GenericConsumer<>));
        
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(settings.Host, settings.VirtualHost, h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                foreach (var queue in settings.Queues.Values)
                {
                    cfg.ReceiveEndpoint(queue.Name, e =>
                    {
                        var eventType = Type.GetType("PocCQRS.Application.Events." + queue.Name);
                        if (eventType == null)
                            throw new InvalidOperationException($"Event type not found: {queue.Name}");
                    
                        var consumerType = typeof(GenericConsumer<>).MakeGenericType(eventType);
                        e.ConfigureConsumer(context, consumerType);
                    });
                }
            });
        });

        services.AddMassTransitHostedService();
        return services;
    }
    // public static IServiceCollection AddMassTransitBus(this IServiceCollection services, IAppSettings appSettings)
    // {
    //     var serviceProvider = services.BuildServiceProvider();
    //     var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    //     var logger = loggerFactory.CreateLogger(nameof(MassTransitBusConfigurator));
    //     
    //     services.AddMassTransit(x =>
    //     {
    //         var settings = appSettings.RabbitMQSettings.Main;
    //         x.AddConsumers(typeof(Consumer<>).Assembly, typeof(GenericConsumer<>).Assembly);
    //         x.UsingRabbitMq((context, cfg) =>
    //         {
    //             cfg.Host(settings.Host, settings.VirtualHost, h =>
    //             {
    //                 h.Username(settings.Username);
    //                 h.Password(settings.Password);
    //             });
    //
    //             foreach (var queue in settings.Queues.Values)
    //             {
    //                 cfg.ReceiveEndpoint(queue.Name, e =>
    //                 {
    //                     logger.LogInformation("Configurando fila: {QueueName}", queue.Name);
    //                     var eventName = "PocCQRS.Application.Events." + queue.Name;
    //                     e.ConfigureConsumer(context, typeof(GenericConsumer<>).MakeGenericType(Type.GetType(eventName) ?? throw new InvalidOperationException("Nome da Fila deve ser especificado")));
    //                 });
    //             }
    //         });
    //     });
    //
    //     services.AddMassTransitHostedService();
    //     
    //     return services;
    // }
}