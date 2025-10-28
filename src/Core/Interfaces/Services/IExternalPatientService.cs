using Core.Models;

namespace Core.Interfaces.Services;

public interface IExternalPatientService
{
    Task<string> GetPatientIdAsync(PatientIdSearchRequest request);

    Task<bool> UpdatePhoneNumberInLpuAsync(PatientPhoneUpdateRequest request);
}