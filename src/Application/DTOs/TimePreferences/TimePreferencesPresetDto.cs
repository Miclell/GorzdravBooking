namespace Application.DTOs.TimePreferences;

public record TimePreferencesPresetDto(
    string Name,
    Guid PatientProfileId,
    bool AnyTime,
    IReadOnlyList<TimePreferenceDto> Preferences
);