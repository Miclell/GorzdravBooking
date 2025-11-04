using Application.Common.Results;
using Application.DTOs.Appointment;

namespace Application.Services.Interfaces;

public interface IAppointmentService
{
    Task<Result<Guid>> CreateAsync(CreateAppointmentDto createDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<AppointmentListItemDto>>> GetByUserAsync(Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<AppointmentDto>>> GetByPatientAsync(Guid patientProfileId,
        CancellationToken cancellationToken = default);

    Task<Result<AppointmentDto>> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);
}