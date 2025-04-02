using MassTransit;
using MediatR;
using PocCQRS.Application.Commands;
using PocCQRS.Application.Events;
using PocCQRS.Domain.Services;

namespace PocCQRS.EntryPoint.EndPoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this WebApplication app)
    {
        app.MapPost("/orders", async (CreateOrderCommand command, IMediator mediator) =>
            Results.Ok(await mediator.Send(command)));

        app.MapGet("/orders/{id}", async (Guid id, IOrderService service) =>
            await service.GetOrderAsync(id));
    }
}