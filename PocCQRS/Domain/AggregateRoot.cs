using PocCQRS.Domain.Events;

namespace PocCQRS.Domain;

public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }
    public int Version { get; protected set; } = -1;
    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent @event)
    {
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public abstract void Apply(IDomainEvent @event);
}
