using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Appointment>> GetByPatientProfileIdAsync(Guid patientProfileId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Appointment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}