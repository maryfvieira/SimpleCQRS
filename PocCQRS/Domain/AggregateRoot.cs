using PocCQRS.Domain.Events;

namespace PocCQRS.Domain;

public abstract class AggregateRoot<TAggregateSnapshot>
{
    public Guid AggregateId { get; set; }
    public int Version { get; set; } = -1;

    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

    public List<IDomainEvent> DomainEvents => _domainEvents;

    public Guid LastEventId { get; set; }

    public abstract TAggregateSnapshot Snapshot { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime LastUpdateAt { get; set; }

    public void AddDomainEvent(IDomainEvent @event)
    {
        _domainEvents.Add(@event);
        LastEventId = @event.EventId;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public abstract void Apply(IDomainEvent @event);
}
