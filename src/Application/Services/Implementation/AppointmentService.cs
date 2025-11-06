using Application.Abstract;
using Application.Common.Results;
using Application.DTOs.Appointment;
using Application.Services.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.Logging;
using Appointment = Core.Entities.Appointment;

namespace Application.Services.Implementation;

public class AppointmentService(
    IAppointmentRepository appointmentRepository,
    IExternalAppointmentService externalAppointmentService,
    ILogger<AppointmentService> logger) : IAppService, IAppointmentService
{
    public async Task<Result<Guid>> CreateAsync(CreateAppointmentDto createDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = new Appointment
            {
                PatientProfileId = createDto.PatientProfileId,
                AppointmentId = createDto.AppointmentId,
                VisitStart = createDto.VisitStart,
                VisitEnd = createDto.VisitEnd,
                Address = createDto.Address,
                Number = createDto.Number,
                Room = createDto.Room,
                Speciality = createDto.Speciality,
                Doctor = createDto.Doctor
            };

            await appointmentRepository.AddAsync(appointment, cancellationToken);

            return Result.Success(appointment.Id);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при создании записи на прием для пациента {PatientProfileId}",
                createDto.PatientProfileId);
            return Error.Failure(e.ToString(), "Failed to create appointment");
        }
    }

    public async Task<Result> DeleteAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null)
                return Error.Failure("Appointment.NotFound", "Appointment not found");

            var cancelRequest = new AppointmentСancelRequest
            {
                AppointmentId = appointment.AppointmentId,
                LpuId = appointment.PatientProfile.LpuId,
                PatientId = appointment.PatientProfile.PatientId,
                EsiaId = ""
            };

            var externalCancelSuccess = await externalAppointmentService.CancelAppointmentAsync(cancelRequest);

            if (!externalCancelSuccess)
            {
                logger.LogWarning(
                    "Не удалось отменить запись во внешней системе. AppointmentId: {AppointmentId}, ExternalId: {ExternalAppointmentId}",
                    appointmentId, appointment.AppointmentId);
                return Error.Failure("External.Cancel.Failed", "Failed to cancel appointment in external system");
            }

            await appointmentRepository.DeleteAsync(appointmentId, cancellationToken);

            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при удалении записи на прием {AppointmentId}", appointmentId);
            return Error.Failure(e.ToString(), "Failed to delete appointment");
        }
    }

    public async Task<Result<IEnumerable<AppointmentListItemDto>>> GetByUserAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var appointments = await appointmentRepository.GetByUserIdAsync(userId, cancellationToken);

            var dtos = appointments.Select(appointment => new AppointmentListItemDto(
                appointment.Id,
                appointment.VisitStart,
                appointment.VisitEnd,
                appointment.Address,
                appointment.Number,
                $"{appointment.PatientProfile.PatientLastName} {appointment.PatientProfile.PatientFirstName} {appointment.PatientProfile.PatientMiddleName}",
                appointment.PatientProfile.LpuShortName,
                appointment.Doctor
            ));

            return Result.Success(dtos);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении записей пользователя {UserId}", userId);
            return Error.Failure(e.ToString(), "Failed to get user appointments");
        }
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> GetByPatientAsync(Guid patientProfileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var appointments =
                await appointmentRepository.GetByPatientProfileIdAsync(patientProfileId, cancellationToken);

            var dtos = appointments.Select(appointment => new AppointmentDto(
                appointment.Id,
                appointment.PatientProfileId,
                appointment.AppointmentId,
                appointment.VisitStart,
                appointment.VisitEnd,
                appointment.Address,
                appointment.Number,
                appointment.Room,
                $"{appointment.PatientProfile.PatientLastName} {appointment.PatientProfile.PatientFirstName} {appointment.PatientProfile.PatientMiddleName}",
                appointment.PatientProfile.LpuShortName
            ));

            return Result.Success(dtos);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении записей пациента {PatientProfileId}", patientProfileId);
            return Error.Failure(e.ToString(), "Failed to get patient appointments");
        }
    }

    // Дополнительный метод для получения конкретной записи
    public async Task<Result<AppointmentDto>> GetByIdAsync(Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null)
                return Error.Failure("Appointment.NotFound", "Appointment not found");

            var dto = new AppointmentDto(
                appointment.Id,
                appointment.PatientProfileId,
                appointment.AppointmentId,
                appointment.VisitStart,
                appointment.VisitEnd,
                appointment.Address,
                appointment.Number,
                appointment.Room,
                $"{appointment.PatientProfile.PatientLastName} {appointment.PatientProfile.PatientFirstName} {appointment.PatientProfile.PatientMiddleName}",
                appointment.PatientProfile.LpuShortName
            );

            return Result.Success(dto);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении записи {AppointmentId}", appointmentId);
            return Error.Failure(e.ToString(), "Failed to get appointment");
        }
    }
}