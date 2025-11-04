using System.Text;
using Core.Events;
using Core.Events.Common;

namespace Infrastructure.Services;

public class HeaderStateService :
    IEventHandler<SearchRequestStarted>,
    IEventHandler<SearchRequestCompleted>,
    IEventHandler<SearchServiceStatusChanged>,
    IEventHandler<NextSearchScheduled>
{
    private readonly StringBuilder _lastRequests = new("-----");
    private int _activeRequests;
    private bool _isConnected;
    private bool _isRunning;

    // Геттеры для UI
    public string ActiveRequests => _activeRequests.ToString();
    public string Status => _isRunning ? _isConnected ? "•••" : "••" : "•";
    public string NextRequestTime { get; private set; } = "∞";

    public string LastRequests
    {
        get
        {
            lock (_lastRequests)
            {
                return _lastRequests.ToString();
            }
        }
    }

    public Task Handle(NextSearchScheduled @event, CancellationToken cancellationToken = default)
    {
        var timeUntil = @event.NextTime - DateTime.Now;
        NextRequestTime = timeUntil.TotalMinutes < 1 ? "now" : $"{(int)timeUntil.TotalMinutes}м";
        OnStateChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task Handle(SearchRequestCompleted @event, CancellationToken cancellationToken = default)
    {
        Interlocked.Decrement(ref _activeRequests);

        lock (_lastRequests)
        {
            _lastRequests.Remove(0, 1);
            _lastRequests.Append(@event.Success ? "✓" : "×");
        }

        OnStateChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task Handle(SearchRequestStarted @event, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _activeRequests);
        OnStateChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task Handle(SearchServiceStatusChanged @event, CancellationToken cancellationToken = default)
    {
        _isRunning = @event.IsRunning;
        _isConnected = @event.IsConnected;
        OnStateChanged?.Invoke();
        return Task.CompletedTask;
    }

    public event Action? OnStateChanged;
}