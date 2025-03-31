using MassTransit;
using PocCQRS.Application.Commands;
using PocCQRS.Application.Events;
using PocCQRS.Domain.Services;

namespace PocCQRS.EntryPoint.EndPoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this WebApplication app)
    {
        app.MapPost("/orders", async (CreateOrder.Command command, IOrderService service) =>
            Results.Ok(await service.CreateOrderAsync(command)));

        app.MapGet("/orders/{id}", async (Guid id, IOrderService service) =>
            await service.GetOrderAsync(id));
    }
}