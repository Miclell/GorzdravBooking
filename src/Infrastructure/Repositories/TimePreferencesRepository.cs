using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TimePreferencesRepository(AppDbContext context) : ITimePreferencesRepository
{
    public async Task<TimePreference?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TimePreferences
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TimePreference>> GetByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await context.TimePreferences
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TimePreference preference, CancellationToken cancellationToken = default)
    {
        await context.TimePreferences.AddAsync(preference, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<TimePreference> preferences,
        CancellationToken cancellationToken = default)
    {
        await context.TimePreferences.AddRangeAsync(preferences, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TimePreference preference, CancellationToken cancellationToken = default)
    {
        context.TimePreferences.Update(preference);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<TimePreference>> GetByPresetAsync(Guid userId, string name,
        CancellationToken cancellationToken = default)
    {
        return await context.TimePreferences
            .Where(tp => tp.UserId == userId && tp.Name == name)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteByPresetAsync(Guid userId, string name, CancellationToken cancellationToken = default)
    {
        var preferences = context.TimePreferences
            .Where(tp => tp.UserId == userId && tp.Name == name);

        context.TimePreferences.RemoveRange(preferences);
        await context.SaveChangesAsync(cancellationToken);
    }
}