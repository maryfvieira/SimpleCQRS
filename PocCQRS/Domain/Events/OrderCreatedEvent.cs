namespace PocCQRS.Domain.Events;

public record OrderCreatedEvent(Guid OrderId, IEnumerable<OrderItemEvent> OrderItems) : IDomainEvent
{
    public string EventStatus => "Created";
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OcurredIn { get; set; } = DateTime.UtcNow;
    public IEnumerable<OrderItemEvent> OrderItems { get; init; } = OrderItems;

}

public record OrderItemEvent(int ProductId, double UnitPrice, int Quantity);