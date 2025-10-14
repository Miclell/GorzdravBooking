using Application.Common.Results;
using Application.DTOs.Appointment;

namespace Application.Coordinators.Interfaces;

public interface IAppointmentCoordinator
{
    Task<Result<bool>> CreateCompleteAppointmentAsync(Core.Entities.AppointmentSearchRequest request, CancellationToken cancellationToken = default);
}