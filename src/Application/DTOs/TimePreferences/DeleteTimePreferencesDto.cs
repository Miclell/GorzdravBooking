namespace Application.DTOs.TimePreferences;

public record DeleteTimePreferencesDto(
    Guid PatientProfileId,
    string Name
);