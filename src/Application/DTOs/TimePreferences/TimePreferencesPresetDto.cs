namespace Application.DTOs.TimePreferences;

public record TimePreferencesPresetDto(
    string Name,
    Guid UserId,
    IReadOnlyList<TimePreferenceDto> Preferences,
    List<DateOnly> ExcludedDates,
    int MaxDaysAhead,
    int MinHoursFromNow
);