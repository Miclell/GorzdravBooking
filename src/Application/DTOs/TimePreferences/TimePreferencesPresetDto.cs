namespace Application.DTOs.TimePreferences;

public record TimePreferencesPresetDto(
    string Name,
    Guid UserId,
    bool AnyTime,
    IReadOnlyList<TimePreferenceDto> Preferences
);