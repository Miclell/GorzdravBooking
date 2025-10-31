using Core.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PatientRepository(AppDbContext context) : IPatientRepository
{
    public async Task<PatientProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.PatientProfiles
            .Include(p => p.AppointmentSearchRequests)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PatientProfile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.PatientProfiles
            .Where(p => p.UserId == userId)
            .Include(p => p.AppointmentSearchRequests)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PatientProfile profile, CancellationToken cancellationToken = default)
    {
        await context.PatientProfiles.AddAsync(profile, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PatientProfile profile, CancellationToken cancellationToken = default)
    {
        context.PatientProfiles.Update(profile);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await context.PatientProfiles.FindAsync([id], cancellationToken);
        if (profile != null)
        {
            context.PatientProfiles.Remove(profile);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}