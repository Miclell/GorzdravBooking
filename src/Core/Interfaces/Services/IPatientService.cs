using Core.Models;

namespace Core.Interfaces.Services;

public interface IPatientService
{
    Task<string> GetPatientIdAsync(PatientIdSearchRequest request);

    Task<bool> UpdatePhoneNumberInLpuAsync(PatientPhoneUpdateRequest request);
}