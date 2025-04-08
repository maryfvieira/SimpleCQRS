using PocCQRS.Domain.Events;
using PocCQRS.Domain.Models;
using PocCQRS.Domain.Services;

namespace PocCQRS.Application.EventHandlers;

public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly IOrderService _service;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(IOrderService service, ILogger<OrderCreatedEventHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        _logger.LogInformation($"Pedido criado: {{OrderId}} {Environment.NewLine} {{EventData}}", @event.OrderId, System.Text.Json.JsonSerializer.Serialize(@event));

        var orderItems = @event.OrderItems.Select(oi => new OrderItemDto(Guid.NewGuid(), @event.OrderId, oi.ProductId, oi.Quantity, oi.UnitPrice));
        var orderQuantity = @event.OrderItems.Sum(oi => oi.Quantity);
        var orderAmount = @event.OrderItems.Sum(oi => oi.UnitPrice);

        await _service.CreateOrderAsync(new Domain.Models.OrderDto(@event.OrderId,orderQuantity,orderAmount, orderItems, @event.EventStatus, @event.OcurredIn));

        await Task.CompletedTask;
    }
}
