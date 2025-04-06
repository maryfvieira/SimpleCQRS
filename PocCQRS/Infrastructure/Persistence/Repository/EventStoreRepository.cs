using PocCQRS.Domain;

namespace PocCQRS.Infrastructure.Persistence.Repository;

public class EventStoreRepository:IEventStoreRepository
{
    private readonly IEventStore _eventStore;

    public EventStoreRepository(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<OrderAggregate?> GetAsync(Guid aggregateId)
    {
        var events = await _eventStore.GetEventsAsync(aggregateId);
        if (events.Count == 0)
            return null;

        var order = new OrderAggregate();
        foreach (var @event in events)
        {
            order.Apply(@event);
        }

        order.ClearDomainEvents();
        return order;
    }

    public async Task SaveAsync(OrderAggregate orderAggregate)
    {
        await _eventStore.SaveEventsAsync(orderAggregate.Id, orderAggregate.DomainEvents, orderAggregate.Version);
        orderAggregate.ClearDomainEvents();
    }
}