using Application.Common.Results;
using Core.Entities;

namespace Application.Coordinators.Interfaces;

public interface IAppointmentCoordinator
{
    Task<Result<bool>> CreateCompleteAppointmentAsync(AppointmentSearchRequest request,
        CancellationToken cancellationToken = default);
}