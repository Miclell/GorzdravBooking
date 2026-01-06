using Application.DTOs.Appointment;
using Application.DTOs.AppointmentSearchRequest;
using Core.Entities;
using Core.Models;
using Appointment = Core.Models.Appointment;

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
            request.DoctorNames,
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
    
    public static CreateAppointmentDto ToCreateAppointmentDto(this AppointmentSearchRequest request,  Appointment appointment, string doctorName) =>
        new(
            request.PatientProfileId,
            appointment.Id,
            appointment.VisitStart,
            appointment.VisitEnd,
            appointment.Address,
            appointment.Number,
            appointment.Room,
            request.Speciality,
            doctorName
        );

    public static AppointmentCreateRequest ToAppointmentCreateRequest(this AppointmentSearchRequest request, Appointment appointment) =>
        new()
        {
            EsiaId = null,
            LpuId = request.PatientProfile.LpuId,
            PatientId = request.PatientProfile.PatientId,
            AppointmentId = appointment.Id,
            ReferralId = null,
            IpmpiCardId = null,
            RecipientEmail = request.PatientProfile.RecipientEmail,
            PatientLastName = request.PatientProfile.PatientLastName,
            PatientFirstName = request.PatientProfile.PatientFirstName,
            PatientMiddleName = request.PatientProfile.PatientMiddleName,
            PatientBirthdate = request.PatientProfile.PatientBirthdate,
            Room = appointment.Room,
            Address = request.PatientProfile.LpuAddress,
            VisitDate = appointment.VisitStart
        };
}