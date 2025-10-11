using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface IPatientRepository
{
    Task<PatientProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PatientProfile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(PatientProfile profile, CancellationToken cancellationToken = default);
    Task UpdateAsync(PatientProfile profile, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}