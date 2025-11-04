using Application.Common.Results;
using Application.DTOs.TimePreferences;

namespace Application.Services.Interfaces;

public interface ITimePreferencesService
{
    Task<Result<List<Guid>>> CreateRangeAsync(IEnumerable<CreateTimePreferenceDto> dtos,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteByPresetAsync(DeleteTimePreferencesDto dto, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<TimePreferencesPresetDto>>> GetByUserAsync(Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<TimePreferencesPresetDto>> GetByPresetAsync(Guid userId, string name,
        CancellationToken cancellationToken = default);
}