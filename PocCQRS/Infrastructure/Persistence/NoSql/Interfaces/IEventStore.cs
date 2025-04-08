using PocCQRS.Domain;
using PocCQRS.Domain.Events;

namespace PocCQRS.Infrastructure.Persistence.NoSql.Interfaces;

public interface IEventStore
{
    Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion);
    Task<List<IDomainEvent>> GetEventsAsync(Guid aggregateId);

    Task SaveSnapshotAsync(Guid aggregateId, Guid lastEventId, ISnapshot snapshotData, int version, DateTime createdAt, DateTime lastUpdateAt);

    Task<TAggregate?> LoadAggregateAsync<TAggregate, TAggregateState>(Guid aggregateId)
        where TAggregate : AggregateRoot<TAggregateState>
        where TAggregateState : ISnapshot, new();
}
