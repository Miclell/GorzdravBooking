using Core.Enums;

namespace Application.DTOs.TimePreferences;

public record TimePreferencesPresetDto(
    string Name,
    Guid UserId,
    TimeSelectionMode TimeMode,
    IReadOnlyList<TimePreferenceDto> Preferences,
    List<DateOnly> ExcludedDates,
    int MaxDaysAhead,
    int MinHoursFromNow
);