using PocCQRS.Domain;
using PocCQRS.Domain.Events;
using PocCQRS.Infrastructure.Persistence.NoSql.Interfaces;
using System.Dynamic;

namespace PocCQRS.Infrastructure.Persistence.NoSql.Repository;

public class EventStoreRepository : IEventStoreRepository
{
    private readonly IEventStore _eventStore;

    public EventStoreRepository(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<OrderAggregate?> GetEventAsync(Guid aggregateId)
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

    public async Task SaveEventAsync(OrderAggregate orderAggregate)
    {
        await _eventStore.SaveEventsAsync(orderAggregate.AggregateId, orderAggregate.DomainEvents, orderAggregate.Version);
        orderAggregate.ClearDomainEvents();
    }

    public async Task SaveSnapshotAsync<TAggregate, TAggregateState>(TAggregate aggregate) 
        where TAggregate : AggregateRoot<TAggregateState>
        where TAggregateState : ISnapshot
    {

        await _eventStore.SaveSnapshotAsync(
            aggregate.AggregateId, aggregate.LastEventId, (dynamic)aggregate.Snapshot,
            aggregate.Version, aggregate.CreateAt, aggregate.LastUpdateAt);

        aggregate.ClearDomainEvents();
    }

    public async Task<TAggregate?> LoadAggregateAsync<TAggregate, TAggregateState>(Guid aggregateId)
        where TAggregate : AggregateRoot<TAggregateState>
        where TAggregateState : ISnapshot, new()
    {
        return await _eventStore.LoadAggregateAsync<TAggregate, TAggregateState>(aggregateId);
    }
}