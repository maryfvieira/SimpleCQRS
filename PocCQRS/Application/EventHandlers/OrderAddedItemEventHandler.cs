using PocCQRS.Domain.Events;

namespace PocCQRS.Application.EventHandlers;

public class OrderAddedItemEventHandler : IEventHandler<OrderAddedItemEvent>
{
    private readonly ILogger<OrderAddedItemEventHandler> _logger;

    public OrderAddedItemEventHandler(ILogger<OrderAddedItemEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderAddedItemEvent @event)
    {
        _logger.LogInformation("Pedido criado: {OrderId}", @event.OrderId);
        return Task.CompletedTask;
    }
}
