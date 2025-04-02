namespace PocCQRS.Infrastructure.Messaging;

public interface IPublisherFactory
{
    Publisher<T> CreatePublisher<T>() where T : class;
}