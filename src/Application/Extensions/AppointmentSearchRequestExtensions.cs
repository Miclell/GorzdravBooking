using Application.DTOs.AppointmentSearchRequest;
using Core.Entities;

namespace Application.Extensions;

public static class AppointmentSearchRequestExtensions
{
    public static AppointmentSearchRequestDto ToDto(this AppointmentSearchRequest request) =>
        new(
            request.Id,
            request.PatientProfileId,
            request.LpuName,
            request.Speciality,
            request.DoctorMode,
            request.DoctorName,
            request.TimeMode,
            request.TimePreferencesPresetName,
            request.SearchInterval,
            request.SpecificStartPoints,
            request.MaxDaysToSearch,
            request.ViewOnly,
            request.Status.ToString(),
            request.CreatedAt,
            request.LastSearchAttempt,
            request.AttemptCount,
            $"{request.PatientProfile.PatientLastName} {request.PatientProfile.PatientFirstName} {request.PatientProfile.PatientMiddleName}",
            request is ReferralSearchRequest r ? r.ReferralNumber : null,
            request switch
            {
                ReferralSearchRequest => "Referral",
                ManualSearchRequest => "Manual",
                _ => "Unknown"
            });
}