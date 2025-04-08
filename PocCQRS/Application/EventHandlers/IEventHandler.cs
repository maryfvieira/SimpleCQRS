using PocCQRS.Domain.Events;

namespace PocCQRS.Application.EventHandlers;

public interface IEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent @event);
}
