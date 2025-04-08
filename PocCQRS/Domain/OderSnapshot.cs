using MassTransit;
using PocCQRS.Domain.Events;

namespace PocCQRS.Domain;
public interface ISnapshot
{

}

public class OrderSnapshot : ISnapshot
{
    public Guid OrderId { get; set; }
    public int Quantity { get;  set; }
    public double Amount { get; set; }
    public string Status { get; set; } = string.Empty;    
    public List<OrderItemSnapshot> OrderItems { get; set; } = new List<OrderItemSnapshot>();
    public DateTime CreateAt { get; set; }
    public DateTime LastUpdateAt { get; set; }

    public class OrderItemSnapshot
    {
        public OrderItemSnapshot(int productId, double unitPrice, int quantity)
        {
                ProductId = productId;
                UnitPrice = unitPrice;
                Quantity = quantity;
        }
        public double UnitPrice { get; init; }
        public int Quantity { get;  init; }
        public int ProductId { get; init; }
    }

    public void Apply(OrderCreatedEvent @event)
    {
        OrderId = @event.OrderId;
        OrderItems = @event.OrderItems.Select(oi => new OrderSnapshot.OrderItemSnapshot(oi.ProductId, oi.UnitPrice, oi.Quantity)).ToList();
        Quantity = @event.OrderItems.Sum(p => p.Quantity);
        Amount = @event.OrderItems.Sum(p => p.UnitPrice * p.Quantity);
        Status = @event.EventStatus;
        CreateAt = DateTime.UtcNow;
        LastUpdateAt = DateTime.UtcNow;
    }

    public void Apply(OrderAddedItemEvent @event)
    {
        OrderItems.AddRange(@event.OrderItems.Select(oi => new OrderSnapshot.OrderItemSnapshot(oi.ProductId, oi.UnitPrice, oi.Quantity)));
        
        Quantity += @event.OrderItems.Sum(p => p.Quantity);
        Amount += @event.OrderItems.Sum(p => p.UnitPrice * p.Quantity);
        Status = @event.EventStatus;
        LastUpdateAt = DateTime.UtcNow;
    }
}