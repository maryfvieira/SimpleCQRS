using PocCQRS.Domain.Events;

namespace PocCQRS.Domain;

public class OrderState
{
    public Guid OrderId { get; set; }
    public int Quantity { get;  set; }
    public double Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public IList<OrderItemState> OrderItemStates { get; set; } = new List<OrderItemState>();
    
    public class OrderItemState
    {
        public OrderItemState(int productId, double unitPrice, int quantity)
        {
                ProductId = productId;
                UnitPrice = unitPrice;
                Quantity = quantity;
        }
        public double UnitPrice { get; init; }
        public int Quantity { get;  init; }
        public int ProductId { get; init; }
    }

    public void Apply(OrderCreatedEvent evt)
    {
        OrderId = evt.OrderId;
        Quantity = evt.OrderItems.Sum(oi => oi.Quantity);
        Amount = evt.OrderItems.Sum(oi => oi.UnitPrice);
    }
}