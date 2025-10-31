using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface ITimePreferencesRepository
{
    Task<TimePreferences?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimePreferences>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(TimePreferences preferences, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TimePreferences> preferences, CancellationToken cancellationToken = default);
    Task UpdateAsync(TimePreferences preferences, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimePreferences>> GetByPresetAsync(Guid userId, string name, CancellationToken cancellationToken = default);
    Task DeleteByPresetAsync(Guid userId, string name, CancellationToken cancellationToken = default);
}