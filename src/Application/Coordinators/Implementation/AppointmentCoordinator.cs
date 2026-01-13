using Application.Common.Results;
using Application.Coordinators.Interfaces;
using Application.DTOs.TimePreferences;
using Application.Extensions;
using Application.Services.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Appointment = Core.Models.Appointment;

namespace Application.Coordinators.Implementation;

public class AppointmentCoordinator(
    IExternalAppointmentService externalAppointmentService,
    ITimePreferencesService timePreferencesService,
    IAppointmentService appointmentService,
    TimeProvider timeProvider,
    ILogger<AppointmentCoordinator> logger) : IAppointmentCoordinator
{
    public async Task<Result<bool>> CreateCompleteAppointmentAsync(AppointmentSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Начало создания записи для пациента {PatientId}", request.PatientProfile.PatientId);

            var preferencesResult = await timePreferencesService.GetByPresetAsync(
                request.PatientProfile.UserId,
                request.TimePreferencesPresetName,
                cancellationToken);

            logger.LogDebug("Получены временные предпочтения: {Success}", preferencesResult.IsSuccess);

            if (preferencesResult.IsFailure)
                return preferencesResult.Error;

            var timePreferences = preferencesResult.Value;

            var appointmentResult = await TryFindAndBookAppointmentWithRetryAsync(
                request, timePreferences, cancellationToken);

            return appointmentResult.IsFailure
                ? appointmentResult.Error
                : Result.Success(appointmentResult.Value);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Booking appointment error for patient {PatientId}", request.PatientProfile.PatientId);
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

            var appointments = await GetAppointmentsAsync(request);
            if (appointments.Count == 0)
            {
                logger.LogDebug("Не найдено номерков на попытке {attempt}", attempt);
                return Result.Success(false);
            }

            logger.LogDebug("Получено {count} номерков на попытке {attempt}", appointments.Count, attempt);

            var appointment =
                TryGetPreferAppointment(appointments, timePreferences, timeProvider.GetLocalNow().DateTime);

            if (!appointment.HasValue)
            {
                logger.LogDebug("Не найдено подходящих номерков на попытке {attempt}", attempt);
                return Result.Success(false);
            }

            if (request.ViewOnly)
            {
                logger.LogDebug("Номерок найден в режиме только для просмотра");
                return Result.Success(false);
            }

            logger.LogDebug("Попытка бронирования номерка {AppointmentId}", appointment.Value.Appointment);

            var createRequest = request.ToAppointmentCreateRequest(appointment.Value.Appointment);
            var result = await externalAppointmentService.CreateAppointmentAsync(createRequest);

            if (result.IsSucces)
            {
                await appointmentService.CreateAsync(
                    request.ToCreateAppointmentDto(appointment.Value.Appointment, appointment.Value.Doctor),
                    cancellationToken);
                logger.LogDebug("Запись успешна");
                return Result.Success(true);
            }

            if (result.ErrorCode != 639)
            {
                logger.LogWarning("Критическая ошибка бронирования: {ErrorCode}", result.ErrorCode);
                return Error.Conflict("Failed.Booking", "Failed to booking appointment in external service");
            }

            // TODO если уже есть такая запись просто отменяем запрос

            logger.LogWarning("Нас обогнала бабуля");

            if (attempt >= maxRetries) continue;

            var delay = baseDelayMs * Math.Pow(2, attempt - 1);
            logger.LogDebug("Ждем {Delay}ms перед следующей попыткой", delay);
            await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
        }

        logger.LogWarning("Нас обогнала бабуля!");
        return Error.Failure("Failure.Booking", "Нас обогнала бабуля!");
    }

    private async Task<List<(string Doctor, Appointment Appointment)>> GetAppointmentsAsync(
        AppointmentSearchRequest request)
    {
        if (request is ManualSearchRequest manualSearchRequest)
        {
            var appointments = new List<(string Doctor, Appointment Appointment)>();
            if (request.DoctorMode == DoctorSelectionMode.SpecificDoctorOrRange)
                foreach (var (doctorId, doctorName) in request.DoctorIds!
                             .Zip(request.DoctorNames!))
                {
                    var slots = await externalAppointmentService.GetByDoctorAsync(
                        int.Parse(manualSearchRequest.PatientProfile.LpuId),
                        doctorId);

                    appointments.AddRange(slots.Select(slot => (doctorName, slot)));
                }
            else
                appointments.AddRange(await externalAppointmentService.GetBySpecialityAsync(
                    int.Parse(manualSearchRequest.PatientProfile.LpuId),
                    request.Speciality));

            return appointments;
        }

        if (request is ReferralSearchRequest referralSearchRequest)
        {
            var result = await externalAppointmentService.GetByReferralAsync(
                referralSearchRequest.ReferralNumber,
                referralSearchRequest.PatientProfile.PatientLastName);

            if (request.DoctorMode == DoctorSelectionMode.SpecificDoctorOrRange)
            {
                var appointments = new List<(string Doctor, Appointment Appointment)>();
                foreach (var doctorName in request.DoctorNames!)
                    appointments.AddRange(result.Specialities
                        .SelectMany(s => s.Doctors)
                        .Where(d => string.Equals(d.Name, doctorName, StringComparison.CurrentCultureIgnoreCase))
                        .SelectMany(d => d.Appointments
                            .Select(a => (d.Name, a))));

                return appointments;
            }

            return result.Specialities
                .SelectMany(s => s.Doctors)
                .SelectMany(d => d.Appointments
                    .Select(a => (d.Name, a))).ToList();
        }

        throw new NotSupportedException();
    }

    private static (string Doctor, Appointment Appointment)? TryGetPreferAppointment(
        List<(string Doctor, Appointment Appointment)> appointments,
        TimePreferencesPresetDto timePreferences,
        DateTime dateTimeNow)
    {
        if (timePreferences.TimeMode == TimeSelectionMode.AnyTime && appointments.Count != 0)
            return appointments.First();

        if (appointments.Count == 0)
            return null;

        return appointments
            .Where(a => IsAppointmentMatching(a.Appointment, timePreferences, dateTimeNow))
            .Cast<(string Doctor, Appointment Appointment)?>()
            .FirstOrDefault();
    }

    private static bool IsAppointmentMatching(
        Appointment appointment,
        TimePreferencesPresetDto timePreferences,
        DateTime dateTimeNow)
    {
        var appointmentDate = DateOnly.FromDateTime(appointment.VisitStart);
        var appointmentTime = TimeOnly.FromDateTime(appointment.VisitStart);

        if (!IsWithinTimeConstraints(appointment.VisitStart, timePreferences, dateTimeNow))
            return false;

        if (timePreferences.ExcludedDates.Contains(appointmentDate))
            return false;

        return timePreferences.Preferences.Any(pref =>
            MatchesPattern(appointment.VisitStart, appointmentDate, pref, timePreferences.TimeMode)
            && MatchesTimeWindow(appointmentTime, pref));
    }

    private static bool IsWithinTimeConstraints(
        DateTime appointmentStart,
        TimePreferencesPresetDto timePreferences,
        DateTime dateTimeNow)
    {
        var daysAhead = (appointmentStart - dateTimeNow).Days;
        var hoursFromNow = (appointmentStart - dateTimeNow).TotalHours;

        return daysAhead <= timePreferences.MaxDaysAhead
               && hoursFromNow >= timePreferences.MinHoursFromNow;
    }

    private static bool MatchesPattern(
        DateTime appointmentStart,
        DateOnly appointmentDate,
        TimePreferenceDto preference,
        TimeSelectionMode timeMode)
    {
        return timeMode switch
        {
            TimeSelectionMode.SpecificDates =>
                preference.Date.HasValue && preference.Date.Value == appointmentDate,
            TimeSelectionMode.WeekdayPattern =>
                preference.Day.HasValue && preference.Day.Value == appointmentStart.DayOfWeek,
            _ => false
        };
    }

    private static bool MatchesTimeWindow(TimeOnly appointmentTime, TimePreferenceDto preference)
    {
        if (preference is not { From: not null, To: not null })
            return true;

        return appointmentTime >= preference.From.Value
               && appointmentTime <= preference.To.Value;
    }
}