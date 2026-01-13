using Application.DTOs.AppointmentSearchRequest;
using Core.Enums;

namespace Application.Extensions;

public static class AppointmentMenuExtensions
{
    // CreateDto
    public static string GetDisplayTitle(this CreateAppointmentSearchRequestDto dto) =>
        $"{dto.LpuName} | " +
        $"{GetSpecialityDisplayName(dto.ReferralNumber, dto.Speciality)} | " +
        $"{GetDoctorDisplayName(dto.DoctorMode, dto.DoctorNames)} | " +
        $"{dto.TimePreferencesPresetName}";
    
    // BaseDto
    public static string GetDisplayTitle(this AppointmentSearchRequestDto dto) =>
        $"{dto.PatientFullName} | " +
        $"{dto.LpuName} | " +
        $"{GetSpecialityDisplayName(dto.ReferralNumber, dto.Speciality)} | " +
        $"{GetDoctorDisplayName(dto.DoctorMode, dto.DoctorNames)} | " +
        $"{dto.TimePreferencesPresetName}";
    
    private static string GetSpecialityDisplayName(int? referralNumber, string speciality) =>
        referralNumber.HasValue
            ? $"Направление №{referralNumber.Value}"
            : speciality;
    
    private static string GetDoctorDisplayName(
        DoctorSelectionMode doctorMode, 
        List<string>? doctorNames)
    {
        if (doctorMode == DoctorSelectionMode.AnyOfSpeciality)
            return "Любой врач";
        
        if (doctorNames?.Any() == true)
            return string.Join(", ", doctorNames);
        
        return "Врач не указан";
    }
}