namespace Application.DTOs.TimePreferences;

public record CreateTimePreferenceDto(
    string Name,
    Guid PatientProfileId,
    DayOfWeek? Day,
    TimeOnly? PreferredTimeFrom, 
    TimeOnly? PreferredTimeTo,
    bool AnyTime = false
);