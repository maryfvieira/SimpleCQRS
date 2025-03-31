namespace PocCQRS.Domain.Entities;

public record Order(Guid Id, string ProductName, int Quantity);