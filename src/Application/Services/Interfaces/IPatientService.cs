using Application.Common.Results;
using Application.DTOs.Patient;

namespace Application.Services.Interfaces;

public interface IPatientService
{
    Task<Result<Guid>> Create(CreatePatientDto createPatientDto, CancellationToken cancellationToken = default);
    Task<Result> Delete(Guid patientId, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<BasePatientProfileDto>>> GetByUser(Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> Update(BasePatientProfileDto patientProfile, CancellationToken cancellationToken = default);
}