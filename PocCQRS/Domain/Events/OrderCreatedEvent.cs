namespace PocCQRS.Domain.Events;

public record OrderCreatedEvent(Guid OrderId, IEnumerable<OrderItemEvent> OrderItems) : IDomainEvent
{
    public IEnumerable<OrderItemEvent> OrderItems { get; init; } = new List<OrderItemEvent>();
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OcurredIn { get; set; } = DateTime.UtcNow;
}

public record OrderItemEvent(int ProductId, double UnitPrice, int Quantity);