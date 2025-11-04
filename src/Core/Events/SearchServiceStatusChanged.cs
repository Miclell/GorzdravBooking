using Core.Events.Common;

namespace Core.Events;

public record SearchServiceStatusChanged(bool IsRunning, bool IsConnected) : IEvent;