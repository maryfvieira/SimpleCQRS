using PocCQRS.Domain.Entities;

namespace PocCQRS.Domain.Events;

public record OrderAddedItemEvent(Guid OrderId, IEnumerable<OrderItemEvent> OrderItems) : IDomainEvent
{
    public string EventStatus => "OrderUpdated";

    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OcurredIn { get; set; } = DateTime.UtcNow;
}