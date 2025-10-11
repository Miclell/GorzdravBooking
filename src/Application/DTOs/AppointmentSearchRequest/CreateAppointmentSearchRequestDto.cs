namespace Application.DTOs.AppointmentSearchRequest;

public record CreateAppointmentSearchRequestDto(
    Guid PatientProfileId,
    string LpuName,
    string DoctorId,
    string DoctorName,
    TimeSpan SearchInterval,
    List<DateTime> SpecificStartPoints,
    string TimePreferencesPresetName,
    bool ViewOnly = false,
    int MaxDaysToSearch = 30
);