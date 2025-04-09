using PocCQRS.Domain.Events;

namespace PocCQRS.Application.Services;

public interface IEventReprocessorService<TEvent> where TEvent : class, IDomainEvent
{
    Task ReprocessAllAsync();
}
