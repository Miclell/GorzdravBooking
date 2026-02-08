using Core.Entities.Common;

namespace Core.Interfaces.Services;

public interface IOwnershipChecker
{
    Task<bool> IsOwnerAsync<T>(Guid userId, Guid resourceId)
        where T : class, IOwnedEntity;

    Task<bool> IsPatientOwnerAsync<T>(Guid userId, Guid resourceId)
        where T : class, IPatientOwnedEntity;
}