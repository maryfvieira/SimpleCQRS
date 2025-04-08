using MassTransit;
using PocCQRS.Domain.Entities;
using PocCQRS.Domain.Events;
using PocCQRS.Domain.Services;

namespace PocCQRS.EntryPoint.Consumer;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IOrderService _service;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(IOrderService service, ILogger<OrderCreatedConsumer> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        string orderItems
            = string.Join(
                Environment.NewLine,
                context.Message.OrderItems
                    .Select(oi => $"Product: {oi.ProductId} - Quantity: {oi.Quantity} - Price: {oi.UnitPrice}"));

        _logger.LogDebug(
            $"[Consumer] Order created: {Environment.NewLine} OrderId: {context.Message.OrderId} {Environment.NewLine} OrderItems: {orderItems}");


        await Task.CompletedTask;
    }
}