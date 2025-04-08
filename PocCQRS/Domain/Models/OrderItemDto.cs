namespace PocCQRS.Domain.Models;

public record OrderItemDto(Guid Id, Guid OrderId, int ProductId, int Quantity, double UnitPrice);