using Core.Interfaces.ApiClient;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.ApiClient.Models;

namespace Infrastructure.Services;

public class ExternalSpecialtyService(IApiService apiService) : IExternalSpecialtyService
{
    public async Task<List<MedicalSpeciality>> GetByLpuAsync(int lpuId)
    {
        var response = await apiService.GetAsync<List<MedicalSpeciality>>(GorzdravApiEndpoints.SpecialtiesByLpu(lpuId));

        if (!response.Success)
            throw new HttpRequestException($"Ошибка при получении списка специальностей: {response.Message}");

        return response.Result!;
    }
}