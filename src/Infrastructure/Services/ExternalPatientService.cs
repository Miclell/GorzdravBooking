using Core.Interfaces;
using Core.Interfaces.ApiClient;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.ApiClient.Models;

namespace Infrastructure.Services;

public class ExternalPatientService(IApiService apiService) : IExternalPatientService
{
    public async Task<string> GetPatientIdAsync(PatientIdSearchRequest request)
    {
        var response = await apiService.GetAsync<string>(GorzdravApiEndpoints.PatientIdSearch(request));
        
        if (!response.Success)
            throw new HttpRequestException($"Ошибка при получении id пациента: {response.Message}");

        return response.Result!;
    }

    public async Task<bool> UpdatePhoneNumberInLpuAsync(PatientPhoneUpdateRequest request)
    {
        var response = await apiService.PostAsync<PatientPhoneUpdateRequest, bool>(GorzdravApiEndpoints.PatientPhoneUpdate, request);
        
        if (!response.Success)
            throw new HttpRequestException($"Ошибка при получении id пациента: {response.Message}");

        return response.Result!;
    }
}