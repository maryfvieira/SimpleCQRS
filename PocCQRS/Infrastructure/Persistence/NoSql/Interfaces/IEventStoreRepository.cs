using PocCQRS.Domain;

namespace PocCQRS.Infrastructure.Persistence.NoSql.Interfaces;

public interface IEventStoreRepository
{
    Task<OrderAggregate?> GetEventAsync(Guid orderId);

    Task SaveEventAsync(OrderAggregate order);

    Task SaveSnapshotAsync<TAggregate, TAggregateState>(TAggregate aggregate)
        where TAggregate : AggregateRoot<TAggregateState>
        where TAggregateState : ISnapshot;

    Task<TAggregate?> LoadAggregateAsync<TAggregate, TAggregateState>(Guid aggregateId)
        where TAggregate : AggregateRoot<TAggregateState>
        where TAggregateState : ISnapshot, new();

}