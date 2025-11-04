using Core.Events.Common;

namespace Core.Events;

public record SearchRequestCompleted(Guid RequestId, bool Success) : IEvent;