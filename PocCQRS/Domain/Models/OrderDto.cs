namespace PocCQRS.Domain.Models;

public record OrderDto(Guid Id, int Quantity, double Amount, IEnumerable<OrderItemDto> OrderItems, string Status, DateTime CreatedOn);