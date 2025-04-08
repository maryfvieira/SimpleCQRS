

namespace PocCQRS.Domain.Entities;

public record Order(Guid Id, int Quantity, double Amount, IEnumerable<OrderItem> OrderItems, string Status, DateTime CreatedOn);

public record OrderItem(Guid Id, Guid OrderId, int ProductId, int Quantity, double UnitPrice);