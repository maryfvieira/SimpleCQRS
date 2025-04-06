using PocCQRS.Domain.Events;

namespace PocCQRS.Infrastructure.Persistence;

public interface IEventStore
{
    Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion);
    Task<List<IDomainEvent>> GetEventsAsync(Guid aggregateId);
}
