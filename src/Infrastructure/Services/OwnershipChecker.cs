using Core.Entities.Common;
using Core.Interfaces.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class OwnershipChecker(AppDbContext context) : IOwnershipChecker
{
    public async Task<bool> IsOwnerAsync<T>(Guid userId, Guid resourceId)
        where T : class, IOwnedEntity
    {
        return await context.Set<T>()
            .AnyAsync(e => e.Id == resourceId && e.UserId == userId);
    }

    public async Task<bool> IsPatientOwnerAsync<T>(Guid userId, Guid resourceId)
        where T : class, IPatientOwnedEntity
    {
        return await context.Set<T>()
            .Where(e => e.Id == resourceId)
            .AnyAsync(e => e.PatientProfile.UserId == userId);
    }
}