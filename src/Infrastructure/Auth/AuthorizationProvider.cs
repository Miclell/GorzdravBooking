using Core.Entities.Common;
using Core.Interfaces.Auth;
using Core.Interfaces.Services;

namespace Infrastructure.Auth;

public class AuthorizationProvider(
    IOwnershipChecker ownershipChecker) : IAuthorizationProvider
{
    public async Task<bool> CanAccessAsync<T>(Guid userId, Guid resourceId)
        where T : class
    {
        if (typeof(IOwnedEntity).IsAssignableFrom(typeof(T)))
        {
            var method = typeof(IOwnershipChecker)
                .GetMethod(nameof(IOwnershipChecker.IsOwnerAsync))!
                .MakeGenericMethod(typeof(T));

            var task = (Task<bool>)method.Invoke(ownershipChecker, [userId, resourceId])!;
            return await task;
        }

        if (typeof(IPatientOwnedEntity).IsAssignableFrom(typeof(T)))
        {
            var method = typeof(IOwnershipChecker)
                .GetMethod(nameof(IOwnershipChecker.IsPatientOwnerAsync))!
                .MakeGenericMethod(typeof(T));

            var task = (Task<bool>)method.Invoke(ownershipChecker, [userId, resourceId])!;
            return await task;
        }

        return false;
    }
}