using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface IAppointmentSearchRequestRepository
{
    Task<AppointmentSearchRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppointmentSearchRequest>> GetByPatientProfileIdAsync(Guid patientProfileId, CancellationToken cancellationToken = default);

    Task<IEnumerable<AppointmentSearchRequest>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppointmentSearchRequest>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(AppointmentSearchRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(AppointmentSearchRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}