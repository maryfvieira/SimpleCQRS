namespace PocCQRS.Domain.Events;

public interface IDomainEvent{
    public Guid EventId { get; set; }
    public DateTime OcurredIn { get; set; }
}