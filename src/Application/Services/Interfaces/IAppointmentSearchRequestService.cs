using Application.Common.Results;
using Application.DTOs.AppointmentSearchRequest;

namespace Application.Services.Interfaces;

public interface IAppointmentSearchRequestService
{
    Task<Result<Guid>> CreateAsync(CreateAppointmentSearchRequestDto createDto,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid requestId, CancellationToken cancellationToken = default);

    Task<Result> UpdateTimePreferencesAsync(UpdatePreferencesDto updateDto,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<AppointmentSearchRequestDto>>> GetActiveByUserAsync(Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<AppointmentSearchRequestDto>>> GetByPatientAsync(Guid patientProfileId,
        CancellationToken cancellationToken = default);
}