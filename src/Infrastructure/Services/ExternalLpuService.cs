using Core.Interfaces;
using Core.Interfaces.ApiClient;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.ApiClient.Models;

namespace Infrastructure.Services;

public class ExternalLpuService(IApiService apiService) : IExternalLpuService
{
    public async Task<List<Lpu>> GetByDistrictAsync(string districtId)
    {
        var response = await apiService.GetAsync<List<Lpu>>(GorzdravApiEndpoints.LpusByDistrict(districtId));
        
        if (!response.Success)
            throw new HttpRequestException($"Ошибка при получении списка медицинских организаций: {response.Message}");

        return response.Result!;
    }
}