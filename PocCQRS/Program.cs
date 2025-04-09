using MassTransit;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using PocCQRS.Application.EventHandlers;
using PocCQRS.Domain.Events;
using PocCQRS.Domain.Services;
using PocCQRS.EntryPoint.Consumer;
using PocCQRS.EntryPoint.EndPoints;
using PocCQRS.Infrastructure.Messaging;
using PocCQRS.Infrastructure.Persistence.Cache;
using PocCQRS.Infrastructure.Persistence.NoSql;
using PocCQRS.Infrastructure.Persistence.Sql;
using PocCQRS.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

// Configurações
builder.Services.Configure<PocCQRS.Infrastructure.Settings.RabbitMQ>(builder.Configuration.GetSection(PocCQRS.Infrastructure.Settings.RabbitMQ.SectionName));
builder.Services.Configure<MySqlDB>(builder.Configuration.GetSection(MySqlDB.SectionName));
builder.Services.Configure<Redis>(builder.Configuration.GetSection(Redis.SectionName));
builder.Services.Configure<PocCQRS.Infrastructure.Settings.MongoDB>(builder.Configuration.GetSection(PocCQRS.Infrastructure.Settings.MongoDB.SectionName));
builder.Services.AddSingleton<IAppSettings, AppSettings>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var serviceProvider = builder.Services.BuildServiceProvider();

// Databases
// Sql (MySql - Transaction database)
builder.Services.AddSqlPersistence();

// NoSql (MongoDb - EventStore - Event Sourcing)
builder.Services.AddNoSqlPersistence(serviceProvider.GetRequiredService<IAppSettings>());

// Cache (Redis - EventState - Snapshot)
builder.Services.AddCachePersistence(builder.Configuration);

// MassTransit Configuration
builder.Services.AddMassTransitBus(serviceProvider.GetRequiredService<IAppSettings>());


// Registrando todos os handlers
builder.Services.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();
builder.Services.AddScoped<IEventHandler<OrderAddedItemEvent>, OrderAddedItemEventHandler>();

// Registrando o consumer genérico
builder.Services.AddScoped(typeof(IConsumer<>), typeof(EventConsumer<>));
builder.Services.AddScoped(typeof(IConsumer<>), typeof(DeadLetterEventConsumer<>));

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

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// Configuração global para evitar problemas com DateTime
BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc, BsonType.String));

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

// Map endpoints
app.MapOrdersEndpoints();
app.MapSettingsEndpoints();

// Add redirection from root to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

await app.RunAsync();