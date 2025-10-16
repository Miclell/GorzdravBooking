using Application.Common.Results;
using Application.Coordinators.Interfaces;
using Application.DTOs.Appointment;
using Application.DTOs.TimePreferences;
using Application.Services.Interfaces;
using Core.Models;

namespace Application.Coordinators.Implementation;

public class AppointmentCoordinator(Core.Interfaces.Services.IAppointmentService externalAppointmentService,
    ITimePreferencesService timePreferencesService,
    IAppointmentService appointmentService) : IAppointmentCoordinator
{
    public async Task<Result<bool>> CreateCompleteAppointmentAsync(Core.Entities.AppointmentSearchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var timePreferences = await timePreferencesService.GetByPresetAsync(
                request.PatientProfileId,
                request.TimePreferencesPresetName,
                cancellationToken);
            
            if (timePreferences.IsFailure)
                return timePreferences.Error;
            
            var appointments = await externalAppointmentService.GetByDoctorAsync(
                int.Parse(request.PatientProfile.LpuId),
                request.DoctorId);
            
            var appointment = TryGetPreferAppointment(
                appointments, 
                timePreferences.Value);
        
            if (appointment == null)
                return Result.Success(false);

            var createRequest = new AppointmentCreateRequest
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
                Address = appointment.Address!,
                VisitDate = appointment.VisitStart
            };

            var isSuccess = await externalAppointmentService.CreateAppointmentAsync(createRequest);
            if (!isSuccess && isSuccess) // TODO раскоментить после разбора 
            {
                return Error.Conflict("Failed.Booking", "Failed to boking appointment in external service");
            }
            
            var dto = new CreateAppointmentDto
            {
                PatientProfileId = request.PatientProfileId,
                AppointmentId = appointment.Id,
                VisitStart = appointment.VisitStart,
                VisitEnd = appointment.VisitEnd,
                Address = appointment.Address,
                Number = appointment.Number,
                Room = appointment.Room
            };
            
            await appointmentService.CreateAsync(dto, cancellationToken);
            return Result.Success(true);
        }
        catch(Exception e)
        {
            return Error.Failure(e.ToString(), "Booking appointment error");
        }
    }

    private static Appointment? TryGetPreferAppointment(List<Appointment> appointments, TimePreferencesPresetDto timePreferences)
    {
        if (timePreferences.Preferences.Count == 0 || appointments.Count == 0)
            return null;
    
        if (timePreferences.AnyTime)
            return appointments[0];

        return appointments.FirstOrDefault(appointment => 
            timePreferences.Preferences.Any(preference => 
                (!preference.Day.HasValue || 
                 preference.Day.Value == appointment.VisitStart.DayOfWeek) &&
                (!preference.From.HasValue || 
                 !preference.To.HasValue || 
                 (TimeOnly.FromDateTime(appointment.VisitStart) >= preference.From.Value &&
                  TimeOnly.FromDateTime(appointment.VisitStart) <= preference.To.Value))
            )
        );
    }
}