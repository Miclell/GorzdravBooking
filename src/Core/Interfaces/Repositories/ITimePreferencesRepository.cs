using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface ITimePreferencesRepository
{
    Task<TimePreference?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimePreference>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(TimePreference preference, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TimePreference> preferences, CancellationToken cancellationToken = default);
    Task UpdatePresetAsync(IEnumerable<TimePreference> preferences, CancellationToken cancellationToken = default);

    Task<IEnumerable<TimePreference>> GetByPresetAsync(Guid userId, string name,
        CancellationToken cancellationToken = default);

    Task DeleteByPresetAsync(Guid userId, string name, CancellationToken cancellationToken = default);
}