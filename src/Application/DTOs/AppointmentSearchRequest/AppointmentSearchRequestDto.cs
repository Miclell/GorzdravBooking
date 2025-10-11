namespace Application.DTOs.AppointmentSearchRequest;

public record AppointmentSearchRequestDto(
    Guid Id,
    Guid PatientProfileId,
    string LpuName,
    string DoctorName,
    TimeSpan SearchInterval,
    List<DateTime> SpecificStartPoints,
    string TimePreferencesPresetName,
    bool ViewOnly,
    int MaxDaysToSearch,
    DateTime CreatedAt,
    DateTime? LastSearchAttempt,
    int AttemptCount,
    string Status,
    string PatientFullName  // Для удобства UI
);