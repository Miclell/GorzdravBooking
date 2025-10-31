using Core.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TimePreferencesRepository(AppDbContext context) : ITimePreferencesRepository
{
    public async Task<TimePreferences?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TimePreferences
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TimePreferences>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.TimePreferences
            .Where(t => t.UserId == userId)
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

    public async Task<IEnumerable<TimePreferences>> GetByPresetAsync(Guid userId, string name, CancellationToken cancellationToken = default)
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