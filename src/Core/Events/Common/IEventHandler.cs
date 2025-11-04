namespace Core.Events.Common;

public interface IEventHandler<in T> where T : IEvent
{
    Task Handle(T @event, CancellationToken cancellationToken = default);
}