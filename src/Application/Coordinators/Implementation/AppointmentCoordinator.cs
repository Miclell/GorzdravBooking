using Application.Common.Results;
using Application.Coordinators.Interfaces;
using Application.DTOs.Appointment;
using Application.DTOs.TimePreferences;
using Application.Services.Interfaces;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.Logging;
using Appointment = Core.Models.Appointment;

namespace Application.Coordinators.Implementation;

public class AppointmentCoordinator(
    IExternalAppointmentService externalExternalAppointmentService,
    ITimePreferencesService timePreferencesService,
    IAppointmentService appointmentService,
    ILogger<AppointmentCoordinator> logger) : IAppointmentCoordinator
{
    public async Task<Result<bool>> CreateCompleteAppointmentAsync(AppointmentSearchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("Начало создания записи для пациента {PatientId}", request.PatientProfile.PatientId);
            
            var timePreferences = await timePreferencesService.GetByPresetAsync(
                request.PatientProfile.UserId,
                request.TimePreferencesPresetName,
                cancellationToken);
            
            logger.LogDebug("Получены временные предпочтения: {Success}", timePreferences.IsSuccess);
            
            if (timePreferences.IsFailure)
                return timePreferences.Error;
            
            logger.LogDebug("Поиск номерков для LPU: {LpuId}, врача: {DoctorId}", request.PatientProfile.LpuId, request.DoctorId);
            
            var appointments = await externalExternalAppointmentService.GetByDoctorAsync(
                int.Parse(request.PatientProfile.LpuId),
                request.DoctorId);
            
            logger.LogDebug("Получено {count} номерков", appointments.Count);
            
            var appointment = TryGetPreferAppointment(
                appointments, 
                timePreferences.Value);
        
            if (appointment == null)
            {
                logger.LogDebug("Не найдено подходящих номерков");
                return Result.Success(false);
            }

            if (request.ViewOnly)
            {
                logger.LogDebug("Номерок найден в режиме только для просмотра");
                return Result.Success(false);
            }
            
            logger.LogDebug("Номерок найден");

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

            var isSuccess = await externalExternalAppointmentService.CreateAppointmentAsync(createRequest);
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
                Room = appointment.Room,
                Speciality = "1", // TODO достать эту хуйню 
                Doctor = "2"
            };
            
            await appointmentService.CreateAsync(dto, cancellationToken);
            return Result.Success(true);
        }
        catch(Exception e)
        {
            logger.LogError("Booking appointment error - {e}", e.ToString());
            return Error.Failure(e.ToString(), "Booking appointment error");
        }
    }

    private static Appointment? TryGetPreferAppointment(List<Appointment> appointments, TimePreferencesPresetDto timePreferences)
    {
        if ((timePreferences.Preferences.Count == 0 && !timePreferences.AnyTime) 
            || appointments.Count == 0)
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