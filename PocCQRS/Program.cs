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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Configuração do MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// Database
builder.Services.AddSingleton<IDbConnectionFactory>(_ => 
    new DbConnectionFactory(builder.Configuration.GetConnectionString("MySQL")));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Settings
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));

// MassTransit
builder.Services.AddMassTransit(registrationConfigurator =>
{
    registrationConfigurator.AddConsumer<OrderCreatedConsumer>();
    
    registrationConfigurator.UsingRabbitMq((context, cfg) =>
    {
        var config = context.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
        
        cfg.Host(config.Host, config.VirtualHost, hostConfigurator =>
        {
            hostConfigurator.Username(config.Username);
            hostConfigurator.Password(config.Password);
        });
        
        cfg.ReceiveEndpoint(config.QueueName, e => 
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });
    });
});

// Swagger configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PocCQRS API", Version = "v1" });
});

// Application services
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PocCQRS API V1");
        c.RoutePrefix = "swagger"; // This makes Swagger UI available at root /swagger
    });
}

// Initialize database
await InitializeDatabase(app.Services);

// Map endpoints
app.MapOrdersEndpoints();

// Add redirection from root to Swagger for convenience
app.MapGet("/", () => Results.Redirect("/swagger"));

await app.RunAsync();

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