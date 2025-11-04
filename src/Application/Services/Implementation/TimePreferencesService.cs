using Application.Abstract;
using Application.Common.Results;
using Application.DTOs.TimePreferences;
using Application.Services.Interfaces;
using Core.Entities;
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
            var timePreferences = dtos.Select(dto => new TimePreferences
            {
                Name = dto.Name,
                UserId = dto.UserId,
                Day = dto.Day,
                PreferredTimeFrom = dto.PreferredTimeFrom,
                PreferredTimeTo = dto.PreferredTimeTo,
                AnyTime = dto.AnyTime
            }).ToList();

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
                .Select(group => new TimePreferencesPresetDto(
                    group.Key,
                    group.First().UserId,
                    group.First().AnyTime,
                    group
                        .Select(tp => new TimePreferenceDto(
                            tp.Day,
                            tp.PreferredTimeFrom,
                            tp.PreferredTimeTo
                        ))
                        .ToList()
                        .AsReadOnly()
                ));

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

            var timePreferencesEnumerable = preferences.ToList();
            if (timePreferencesEnumerable.Count == 0)
                return Error.NotFound("Preferences.Not.Found", "Preferences not found");

            var anyTimePreference = timePreferencesEnumerable.FirstOrDefault(p => p.AnyTime);
            var hasAnyTime = anyTimePreference != null;

            var presetDto = new TimePreferencesPresetDto(
                name,
                userId,
                hasAnyTime,
                hasAnyTime
                    ? new List<TimePreferenceDto>().AsReadOnly()
                    : timePreferencesEnumerable
                        .Select(p => new TimePreferenceDto(
                            p.Day,
                            p.PreferredTimeFrom,
                            p.PreferredTimeTo
                        ))
                        .ToList()
                        .AsReadOnly()
            );

            return Result.Success(presetDto);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении пресета - {Name} для пациента {UserId}", name, userId);
            return Error.Failure(e.ToString(), "Failed to get time preferences");
        }
    }
}