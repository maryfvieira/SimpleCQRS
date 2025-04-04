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
builder.Services.AddPersistence();

// MassTransit Configuration
var serviceProvider = builder.Services.BuildServiceProvider();
builder.Services.AddMassTransitBus(serviceProvider.GetRequiredService<IAppSettings>());

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

// Map endpoints
app.MapOrdersEndpoints();
app.MapSettingsEndpoints();

// Add redirection from root to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

await app.RunAsync();



