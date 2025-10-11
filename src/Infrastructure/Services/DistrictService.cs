using Core.Interfaces;
using Core.Interfaces.ApiClient;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.ApiClient;
using Infrastructure.ApiClient.Models;

namespace Infrastructure.Services;

public class DistrictService(IApiService apiService) : IDistrictService
{
    public async Task<List<District>> GetDistrictsAsync()
    {
        var response = await apiService.GetAsync<List<District>>(GorzdravApiEndpoints.Districts);
        
        if (!response.Success)
            throw new HttpRequestException($"Ошибка при получении районов: {response.Message}");

        return response.Result!;
    }
}