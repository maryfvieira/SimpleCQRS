using PocCQRS.Application.Commands;
using PocCQRS.Domain.Events;

namespace PocCQRS.Domain;

public class OrderAggregate : AggregateRoot<OrderSnapshot>
{
    public override OrderSnapshot Snapshot { get; set; }

    public OrderAggregate()
    {
        Snapshot = new OrderSnapshot();
        foreach (var evt in DomainEvents)
        {
            Apply(evt);
            Version++;
        }
    }

    public async Task<bool> CreateOrderAsync(Guid orderId, OrderCreatedEvent @event)
    {
        AggregateId = orderId;
        try
        {            
            Apply(@event);
            AddDomainEvent(@event);
        
            return await Task.FromResult(true);
        }
        catch (Exception e)
        {
            //_logger.LogError("Event state failed - {EventType}", typeof(@event).Name);
            return await Task.FromResult(false);
        }
    }

    public void AddItem(OrderAddedItemEvent @event)
    {
        Apply(@event);
        AddDomainEvent(@event);
    }
    //
    // public void CompleteOrder(Guid orderId)
    // {
    //     if (Snapshot.IsCompleted)
    //         throw new InvalidOperationException("Order is already completed");
    //
    //     var @event = new OrderCompletedEvent(orderId);
    //     Apply(@event);
    //     AddDomainEvent(@event);
    // }

    public sealed override void Apply(IDomainEvent @event)
    {
        //When((dynamic)@event);
        var evt = (dynamic)@event;

        Snapshot.Apply(evt);
        Version++;
    }

    private void When(OrderCreatedEvent @event)
    {

    }

    // private void When(OrderItemAddedEvent @event)
    // {
    //     Snapshot.Items.Add(new OrderItems
    //     {
    //         ProductId = @event.ProductId,
    //         Price = @event.Price,
    //         Quantity = @event.Quantity
    //     });
    // }
    //
    // private void When(OrderCompletedEvent @event)
    // {
    //     Snapshot.Status = "Completed";
    //     Snapshot.IsCompleted = true;
    //     Snapshot.CompletionDate = DateTime.UtcNow;
    // }
}
