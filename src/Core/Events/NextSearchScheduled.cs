using Core.Events.Common;

namespace Core.Events;

public record NextSearchScheduled(DateTime NextTime) : IEvent;