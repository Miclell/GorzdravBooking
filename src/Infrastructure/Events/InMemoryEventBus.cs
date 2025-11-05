using Core.Events.Common;

namespace Infrastructure.Events;

public class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<object>> _handlers = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public void Subscribe<T>(IEventHandler<T> handler) where T : IEvent
    {
        _semaphore.Wait();
        try
        {
            var eventType = typeof(T);
            if (!_handlers.ContainsKey(eventType))
                _handlers[eventType] = [];

            _handlers[eventType].Add(handler);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IEvent
    {
        List<object> handlers;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var eventType = typeof(T);
            if (!_handlers.TryGetValue(eventType, out var handler))
                return;

            handlers = new List<object>(handler);
        }
        finally
        {
            _semaphore.Release();
        }

        var tasks = handlers.Select(handler =>
            ((IEventHandler<T>)handler).Handle(@event, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public void SubscribeMultiple(params object[] handlers)
    {
        foreach (var handler in handlers)
        {
            var handlerType = handler.GetType();
            var eventHandlerInterfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>));

            foreach (var interfaceType in eventHandlerInterfaces)
            {
                var eventType = interfaceType.GetGenericArguments()[0];
                var subscribeMethod = typeof(IEventBus).GetMethod(nameof(Subscribe))!
                    .MakeGenericMethod(eventType);

                subscribeMethod.Invoke(this, [handler]);
            }
        }
    }
}