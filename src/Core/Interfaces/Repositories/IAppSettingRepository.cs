using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface IAppSettingRepository
{
    public Task<AppSetting?> GetAsync(string key, CancellationToken cancellationToken = default);
    public Task AddAsync(AppSetting setting, CancellationToken cancellationToken = default);
    public Task UpdateAsync(AppSetting setting, CancellationToken cancellationToken = default);
    public Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}