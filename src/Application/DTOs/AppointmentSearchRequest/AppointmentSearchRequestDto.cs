using Core.Enums;
using Core.Models;

namespace Application.DTOs.AppointmentSearchRequest;

public record AppointmentSearchRequestDto(
    Guid Id,
    Guid PatientProfileId,
    string LpuName,
    string Speciality,
    DoctorSelectionMode DoctorMode,
    List<string>? DoctorNames,
    TimeSelectionMode TimeMode,
    string? TimePreferencesPresetName,
    TimeSpan SearchInterval,
    List<DateTime>? SpecificStartPoints,
    int MaxDaysToSearch,
    bool ViewOnly,
    string Status,
    DateTime CreatedAt,
    DateTime? LastSearchAttempt,
    int AttemptCount,
    string PatientFullName,
    int? ReferralNumber,
    string RequestType
);