namespace PocCQRS.Domain.Entities;

public record Order(string ProductName, int Quantity, double Amount, string status, DateTime CreatedOn);

public record OrderItem(int OrderItemId);