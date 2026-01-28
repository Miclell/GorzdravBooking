using Core.Enums;

namespace Application.DTOs.AppointmentSearchRequest;

public record AppointmentSearchRequestDto(
    Guid Id,
    Guid PatientProfileId,
    string LpuName,
    string Speciality,
    DoctorSelectionMode DoctorMode,
    List<string>? DoctorNames,
    string TimePreferencesPresetName,
    TimeSpan SearchInterval,
    List<DateTime>? SpecificStartPoints,
    int MaxDaysToSearch,
    bool ViewOnly,
    string Status,
    DateTime CreatedAt,
    DateTime? LastSearchAttempt,
    int AttemptCount,
    string PatientFullName,
    string? ReferralNumber,
    string RequestType
);