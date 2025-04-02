using Dapper;
using MassTransit;
using PocCQRS.EntryPoint.Consumer;
using PocCQRS.EntryPoint.EndPoints;
using MediatR;
using Microsoft.Extensions.Options;
using PocCQRS.Domain.Services;
using PocCQRS.Infrastructure.Persistence;
using PocCQRS.Infrastructure.Persistence.Repository;
using PocCQRS.Infrastructure.Settings;
using Microsoft.OpenApi.Models;
using PocCQRS.Application.Events;
using PocCQRS.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Configurações
builder.Services.Configure<PocCQRS.Infrastructure.Settings.RabbitMQ>(builder.Configuration.GetSection(PocCQRS.Infrastructure.Settings.RabbitMQ.SectionName));
builder.Services.Configure<MySqlDB>(builder.Configuration.GetSection(MySqlDB.SectionName));
builder.Services.Configure<Reddis>(builder.Configuration.GetSection(Reddis.SectionName));
builder.Services.AddSingleton<IAppSettings, AppSettings>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Database
builder.Services.AddSingleton<IDbConnectionFactory>(provider => 
    new DbConnectionFactory(provider.GetRequiredService<IAppSettings>().DatabaseSettings.ConnectionString));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// MassTransit Configuration
var serviceProvider = builder.Services.BuildServiceProvider();
//builder.Services.AddMassTransitBus(serviceProvider.GetRequiredService<IAppSettings>());

// Lista de eventos que terão consumidores registrados dinamicamente
var eventTypes = new[] { typeof(OrderCreatedEvent) };


// MassTransit
var config = serviceProvider.GetRequiredService<IOptions<PocCQRS.Infrastructure.Settings.RabbitMQ>>().Value.Main;
builder.Services.AddMassTransit(registrationConfigurator =>
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

builder.Services.AddSingleton<IPublisherFactory>(provider => 
{
    var logger = provider.GetRequiredService<ILogger<PublisherFactory>>();
    var appSettings = provider.GetRequiredService<IAppSettings>();
    var bus = provider.GetRequiredService<IBus>(); // Changed from IPublishEndpoint to IBus
    
    try
    {
        return new PublisherFactory(logger, appSettings, bus);
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Failed to create PublisherFactory");
        throw;
    }
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PocCQRS API", Version = "v1" });
});

// Application services
builder.Services.AddScoped<IOrderService, OrderService>();
// Configure MassTransit after all services are registered
// var serviceProvider = builder.Services.BuildServiceProvider();
// var busConfigurator = serviceProvider.GetRequiredService<MassTransitBusConfigurator>();
// busConfigurator.Configure();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PocCQRS API V1");
        c.RoutePrefix = "swagger";
    });
}

// Initialize database
await InitializeDatabase(app.Services);

// Map endpoints
app.MapOrdersEndpoints();

// Add redirection from root to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

await app.RunAsync();




//
// // MediatR
// builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
//
// // Database
// builder.Services.AddSingleton<IDbConnectionFactory>(provider => 
//     new DbConnectionFactory(provider.GetRequiredService<IAppSettings>().DatabaseSettings.ConnectionString));
// builder.Services.AddScoped<IOrderRepository, OrderRepository>();
//
// // Swagger
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(c => 
// {
//     c.SwaggerDoc("v1", new OpenApiInfo { Title = "PocCQRS API", Version = "v1" });
// });
//
// // Services
// builder.Services.AddScoped<IOrderService, OrderService>();
//
// // Configure MassTransit after all services are registered
// var serviceProvider = builder.Services.BuildServiceProvider();
// var busConfigurator = serviceProvider.GetRequiredService<MassTransitBusConfigurator>();
// busConfigurator.Configure();
//
// var app = builder.Build();
//
// // Registro dinâmico de consumidores
// var consumerTypes = typeof(Program).Assembly.GetTypes()
//     .Where(t => t.IsClass && !t.IsAbstract && typeof(IConsumer).IsAssignableFrom(t))
//     .ToList();
//
// // MassTransit com configuração genérica
// builder.Services.AddMassTransit(registrationConfigurator =>
// {
//     // Registrar consumidores dinamicamente
//     consumerTypes.ForEach(consumerType => 
//     {
//         registrationConfigurator.AddConsumer(consumerType);
//     });
//     
//     registrationConfigurator.UsingRabbitMq((context, cfg) =>
//     {
//         var settings = context.GetRequiredService<IAppSettings>();
//         var rabbitConfig = settings.RabbitMQSettings.Main;
//         
//         cfg.Host(rabbitConfig.Host, rabbitConfig.VirtualHost, h => 
//         {
//             h.Username(rabbitConfig.Username);
//             h.Password(rabbitConfig.Password);
//         });
//
//         foreach (KeyValuePair<string, PocCQRS.Infrastructure.Settings.RabbitMQ.QueueSettings> queueConfig in rabbitConfig.Queues)
//         {
//             PocCQRS.Infrastructure.Settings.RabbitMQ.QueueSettings queueSettings = queueConfig.Value;
//             var consumerType = consumerTypes.FirstOrDefault(c => 
//                 c.Name.Equals($"{queueConfig.Key}Consumer", StringComparison.OrdinalIgnoreCase));
//             
//             if (consumerType != null)
//             {
//                 cfg.ReceiveEndpoint(queueSettings.Name, (IRabbitMqReceiveEndpointConfigurator e) => 
//                 {
//                     // Configuração dinâmica do consumidor
//                     var configureMethod = typeof(ConsumerExtensions).GetMethod("ConfigureConsumer")
//                         ?.MakeGenericMethod(consumerType);
//                     configureMethod?.Invoke(null, new object[] { e, context });
//     
//                     // Configuração avançada da DLQ
//                     var dlqName = !string.IsNullOrWhiteSpace(queueSettings.DLQ.Queue) 
//                         ? queueSettings.DLQ.Queue 
//                         : $"{queueSettings.Name}-error";
//     
//                     e.BindDeadLetterQueue(queueSettings.DLQ.Exchange, queueSettings.DLQ.Queue, dlq =>
//                     {
//                         dlq.Durable = queueSettings.DLQ.Durable;               // Persistente entre reinícios do broker
//                         dlq.AutoDelete = queueSettings.DLQ.AutoDelete;           // Não remove quando não há consumidores
//                         //dlq.SetQueueArgument("x-queue-type", "quorum"); // Para clusters RabbitMQ
//                         dlq.SetQueueArgument("x-message-ttl", queueSettings.DLQ.TTL); 
//                     });
//     
//                     // Configurações de resiliência
//                     e.UseMessageRetry(r => r.Interval(
//                         queueSettings.RetryCount, 
//                         queueSettings.RetryInterval
//                     ));
//     
//                     if (queueSettings.CircuitBreaker != null)
//                     {
//                         e.UseCircuitBreaker(cb => 
//                         {
//                             cb.TripThreshold = queueSettings.CircuitBreaker.TripThreshold;
//                             cb.ActiveThreshold = queueSettings.CircuitBreaker.ActiveThreshold;
//                             cb.ResetInterval = queueSettings.CircuitBreaker.ResetInterval;
//                             cb.TrackingPeriod = queueSettings.CircuitBreaker.TrackingPeriod;
//                         });
//                     }
//                 });
//             }
//         }
//     });
// });
//
//
//
// // Pipeline
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI(c => 
//     {
//         c.SwaggerEndpoint("/swagger/v1/swagger.json", "PocCQRS API V1");
//         c.RoutePrefix = "swagger";
//     });
// }
//
// await InitializeDatabase(app.Services);
// app.MapOrdersEndpoints();
// app.MapGet("/", () => Results.Redirect("/swagger"));
// app.MapPost("/settings/reload", async (IAppSettings settings) => 
// {
//     await settings.ReloadAsync();
//     return Results.Ok("Configurações recarregadas");
// });
//
// await app.RunAsync();

async Task InitializeDatabase(IServiceProvider services)
{
    try
    {
        using var scope = services.CreateScope();
        var connection = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Orders (
                Id CHAR(36) PRIMARY KEY,
                ProductName VARCHAR(100) NOT NULL,
                Quantity INT NOT NULL,
                CreatedAt DATETIME NOT NULL
            )");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
        throw;
    }
}


Type GetConsumerTypeByQueueName(string queueName)
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