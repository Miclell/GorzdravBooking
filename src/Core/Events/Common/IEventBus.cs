namespace Core.Events.Common;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IEvent;
    void Subscribe<T>(IEventHandler<T> handler) where T : IEvent;
    void SubscribeMultiple(params object[] handlers);
}