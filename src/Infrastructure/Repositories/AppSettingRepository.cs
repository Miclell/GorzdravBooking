using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AppSettingRepository(AppDbContext context) : IAppSettingRepository
{
    public async Task<AppSetting?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return await context.AppSettings.FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
    }

    public async Task AddAsync(AppSetting setting, CancellationToken cancellationToken = default)
    {
        await context.AppSettings.AddAsync(setting, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AppSetting setting, CancellationToken cancellationToken = default)
    {
        context.AppSettings.Update(setting);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await context.AppSettings.FindAsync([key], cancellationToken);
        if (setting != null)
        {
            context.AppSettings.Remove(setting);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}