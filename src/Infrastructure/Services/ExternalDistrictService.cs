using Core.Interfaces.ApiClient;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.ApiClient;

namespace Infrastructure.Services;

public class ExternalDistrictService(IApiService apiService) : IExternalDistrictService
{
    public async Task<List<District>> GetDistrictsAsync()
    {
        var response = await apiService.GetAsync<List<District>>(GorzdravApiEndpoints.Districts);

        if (!response.Success)
            throw new HttpRequestException($"Ошибка при получении районов: {response.Message}");

        return response.Result!;
    }
}