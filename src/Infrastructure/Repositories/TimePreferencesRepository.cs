using Core.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TimePreferencesRepository(AppDbContext context) : ITimePreferencesRepository
{
    public async Task<TimePreferences?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TimePreferences
            .Include(p => p.PatientProfile)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TimePreferences>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.TimePreferences
            .Where(t => t.PatientProfile.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TimePreferences preferences, CancellationToken cancellationToken = default)
    {
        await context.TimePreferences.AddAsync(preferences, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task AddRangeAsync(IEnumerable<TimePreferences> preferences, CancellationToken cancellationToken = default)
    {
        await context.TimePreferences.AddRangeAsync(preferences, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }


    public async Task UpdateAsync(TimePreferences preferences, CancellationToken cancellationToken = default)
    {
        context.TimePreferences.Update(preferences);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByPresetAsync(Guid patientProfileId, string name, CancellationToken cancellationToken = default)
    {
        var preferences = context.TimePreferences
            .Where(tp => tp.PatientProfileId == patientProfileId && tp.Name == name);

        context.TimePreferences.RemoveRange(preferences);
        await context.SaveChangesAsync(cancellationToken);
    }

}