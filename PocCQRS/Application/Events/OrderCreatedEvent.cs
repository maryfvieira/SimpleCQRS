namespace PocCQRS.Application.Events;

public record OrderCreatedEvent(Guid OrderId, string ProductName);