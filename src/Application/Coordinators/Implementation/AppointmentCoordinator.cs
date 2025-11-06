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
    public async Task<Result<bool>> CreateCompleteAppointmentAsync(AppointmentSearchRequest request,
    CancellationToken cancellationToken)
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
    
            var appointmentResult = await TryFindAndBookAppointmentWithRetryAsync(
                request, timePreferences.Value, cancellationToken);
    
            return appointmentResult.IsFailure 
                ? appointmentResult.Error 
                : Result.Success(appointmentResult.Value);
        }
        catch (Exception e)
        {
            logger.LogError("Booking appointment error - {e}", e.ToString());
            return Error.Failure(e.ToString(), "Booking appointment error");
        }
    }
    
    private async Task<Result<bool>> TryFindAndBookAppointmentWithRetryAsync(
        AppointmentSearchRequest request,
        TimePreferencesPresetDto timePreferences,
        CancellationToken cancellationToken)
    {
        const int maxRetries = 3;
        const int baseDelayMs = 1000;
    
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            logger.LogDebug("Попытка {attempt} найти и забронировать номерок", attempt);
    
            var appointments = await externalExternalAppointmentService.GetByDoctorAsync(
                int.Parse(request.PatientProfile.LpuId),
                request.DoctorId);
    
            logger.LogDebug("Получено {count} номерков на попытке {attempt}", appointments.Count, attempt);
    
            var appointment = TryGetPreferAppointment(appointments, timePreferences);
    
            if (appointment == null)
            {
                logger.LogDebug("Не найдено подходящих номерков на попытке {attempt}", attempt);
                return Result.Success(false);
            }
    
            if (request.ViewOnly)
            {
                logger.LogDebug("Номерок найден в режиме только для просмотра");
                return Result.Success(false);
            }
    
            logger.LogDebug("Попытка бронирования номерка {AppointmentId}", appointment.Id);
    
            var createRequest = CreateAppointmentRequest(request, appointment);
            var result = await externalExternalAppointmentService.CreateAppointmentAsync(createRequest);
    
            if (result.IsSucces)
            {
                await SaveAppointmentToDatabase(request, appointment, cancellationToken);
                return Result.Success(true);
            }
    
            if (result.ErrorCode != 639)
            {
                logger.LogWarning("Критическая ошибка бронирования: {ErrorCode}", result.ErrorCode);
                return Error.Conflict("Failed.Booking", "Failed to booking appointment in external service");
            }
    
            logger.LogWarning("Нас обогнала бабуля");

            if (attempt >= maxRetries) continue;
            
            var delay = baseDelayMs * Math.Pow(2, attempt - 1);
            logger.LogDebug("Ждем {delay}ms перед следующей попыткой", delay);
            await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
        }
    
        logger.LogWarning("Нас обогнала бабуля!");
        return Error.Failure("Failure.Booking", "Нас обогнала бабуля!");
    }
    
    private static AppointmentCreateRequest CreateAppointmentRequest(AppointmentSearchRequest request, Appointment appointment)
    {
        return new AppointmentCreateRequest
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
    
    private async Task SaveAppointmentToDatabase(AppointmentSearchRequest request, Appointment appointment, 
        CancellationToken cancellationToken)
    {
        var dto = new CreateAppointmentDto(
            request.PatientProfileId,
            appointment.Id,
            appointment.VisitStart,
            appointment.VisitEnd,
            appointment.Address,
            appointment.Number,
            appointment.Room,
            request.Speciality,
            request.DoctorName
        );
    
        await appointmentService.CreateAsync(dto, cancellationToken);
    }
    
    private static Appointment? TryGetPreferAppointment(List<Appointment> appointments,
        TimePreferencesPresetDto timePreferences)
    {
        if ((timePreferences.Preferences.Count == 0 && !timePreferences.AnyTime)
            || appointments.Count == 0)
            return null;

        if (timePreferences.AnyTime)
            return appointments.First();

        return appointments.FirstOrDefault(appointment =>
            timePreferences.Preferences.Any(preference =>
            {
                if (preference.Day.HasValue &&
                    preference.Day.Value != appointment.VisitStart.DayOfWeek)
                    return false;

                if (preference is not { From: not null, To: not null })
                    return true;

                var appointmentTime = TimeOnly.FromDateTime(appointment.VisitStart);
                return appointmentTime >= preference.From.Value &&
                       appointmentTime <= preference.To.Value;
            })
        );
    }
}