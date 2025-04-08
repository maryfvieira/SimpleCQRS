namespace PocCQRS.Domain.Events;

public interface IDomainEvent{
    string EventStatus { get; }
    Guid EventId { get; set; }
    DateTime OcurredIn { get; set; }
}