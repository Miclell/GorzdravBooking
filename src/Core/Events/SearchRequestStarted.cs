using Core.Events.Common;

namespace Core.Events;

public record SearchRequestStarted(Guid RequestId) : IEvent;