using Application.Abstract;
using Application.Common.Results;
using Application.DTOs.TimePreferences;
using Application.Extensions;
using Application.Services.Interfaces;
using Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementation;

public class TimePreferencesService(
    ITimePreferencesRepository timePreferencesRepository,
    ILogger<TimePreferencesService> logger) : IAppService, ITimePreferencesService
{
    public async Task<Result<List<Guid>>> CreateRangeAsync(IEnumerable<CreateTimePreferenceDto> dtos,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var timePreferences = dtos.Select(dto => dto.ToEntity()).ToList();

            await timePreferencesRepository.AddRangeAsync(timePreferences, cancellationToken);

            return Result.Success(timePreferences.Select(tp => tp.Id).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при создании временных предпочтений");
            return Error.Failure(e.ToString(), "Failed to create time preferences");
        }
    }

    public async Task<Result> DeleteByPresetAsync(DeleteTimePreferencesDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await timePreferencesRepository.DeleteByPresetAsync(
                dto.UserId,
                dto.Name,
                cancellationToken);

            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при удалении пресета {PresetName} для пользователя {UserId}", dto.Name,
                dto.UserId);

            return Error.Failure(e.ToString(), "Failed to delete time preferences");
        }
    }

    public async Task<Result<IEnumerable<TimePreferencesPresetDto>>> GetByUserAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await timePreferencesRepository.GetByUserIdAsync(userId, cancellationToken);

            var presets = entities
                .GroupBy(tp => tp.Name)
                .Select(group =>
                {
                    var first = group.First();

                    return new TimePreferencesPresetDto(
                        first.Name,
                        first.UserId,
                        first.TimeMode,
                        group.Select(tp => tp.ToDto())
                            .ToList()
                            .AsReadOnly(),
                        first.ExcludedDates ?? new List<DateOnly>(),
                        first.MaxDaysAhead,
                        first.MinHoursFromNow
                    );
                });

            return Result.Success(presets);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении временных предпочтений для пользователя {UserId}", userId);
            return Error.Failure(e.ToString(), "Failed to get time preferences");
        }
    }

    public async Task<Result<TimePreferencesPresetDto>> GetByPresetAsync(Guid userId, string name,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await timePreferencesRepository
                .GetByPresetAsync(userId, name, cancellationToken);

            var timePreferencesList = preferences.ToList();
            if (timePreferencesList.Count == 0)
                return Error.NotFound("Preferences.Not.Found", "Preferences not found");

            var first = timePreferencesList.First();
            var preset = new TimePreferencesPresetDto(
                name,
                userId,
                first.TimeMode,
                timePreferencesList.Select(tp => tp.ToDto())
                    .ToList()
                    .AsReadOnly(),
                first.ExcludedDates ?? new List<DateOnly>(),
                first.MaxDaysAhead,
                first.MinHoursFromNow
            );

            return Result.Success(preset);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении пресета - {Name} для пациента {UserId}", name, userId);
            return Error.Failure(e.ToString(), "Failed to get time preferences");
    public async Task<Result> UpdatePresetAsync(List<CreateTimePreferenceDto> dtos,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var timePreferences = dtos.Select(dto => dto.ToEntity()).ToList();
            await timePreferencesRepository.UpdatePresetAsync(timePreferences, cancellationToken);

            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при обновлении пресета {Name} для пользователя {UserId]}", dtos.FirstOrDefault()?.Name, dtos.FirstOrDefault()?.UserId);
            return Error.Failure("UnexpectedError", "Failed to update time preferences");
        }
    }
}