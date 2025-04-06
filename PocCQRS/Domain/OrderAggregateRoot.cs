using PocCQRS.Application.Commands;
using PocCQRS.Domain.Events;

namespace PocCQRS.Domain;

public class OrderAggregate : AggregateRoot
{
    public OrderState State { get; private set; }
    
    public OrderAggregate()
    {
        State = new OrderState();
        foreach (var evt in DomainEvents)
        {
            Apply(evt);
            Version++;
        }
    }

    public async Task<bool> CreateOrderAsync(Guid orderId, OrderCreatedEvent @event)
    {
        Id = orderId;
        try
        {
            Apply(@event);
            AddDomainEvent(@event);
        
            return await Task.FromResult(true);
        }
        catch (Exception e)
        {
            //_logger.LogError("Event state failed - {EventType}", typeof(@event).Name);
            return  await  Task.FromResult(false);
        }
    }

    // public void AddItem(Guid orderId, Guid productId, string productName, decimal price, int quantity)
    // {
    //     var @event = new OrderItemAddedEvent(orderId, productId, productName, price, quantity);
    //     Apply(@event);
    //     AddDomainEvent(@event);
    // }
    //
    // public void CompleteOrder(Guid orderId)
    // {
    //     if (State.IsCompleted)
    //         throw new InvalidOperationException("Order is already completed");
    //
    //     var @event = new OrderCompletedEvent(orderId);
    //     Apply(@event);
    //     AddDomainEvent(@event);
    // }

    public sealed override void Apply(IDomainEvent @event)
    {
        When((dynamic)@event);
        Version++;
    }

    private void When(OrderCreatedEvent @event)
    {
        State.OrderId = @event.OrderId;
        State.OrderItemStates = @event.OrderItems.Select(oi => new OrderState.OrderItemState(oi.ProductId, oi.UnitPrice, oi.Quantity)).ToList();
        State.Quantity = @event.OrderItems.Sum(p => p.Quantity);
        State.Amount = @event.OrderItems.Sum(p => p.UnitPrice);
        State.Status = "Created";
    }

    // private void When(OrderItemAddedEvent @event)
    // {
    //     State.Items.Add(new OrderItems
    //     {
    //         ProductId = @event.ProductId,
    //         ProductId = @event.ProductId,
    //         Price = @event.Price,
    //         Quantity = @event.Quantity
    //     });
    // }
    //
    // private void When(OrderCompletedEvent @event)
    // {
    //     State.Status = "Completed";
    //     State.IsCompleted = true;
    //     State.CompletionDate = DateTime.UtcNow;
    // }
}
