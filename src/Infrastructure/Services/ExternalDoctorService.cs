using Core.Interfaces.ApiClient;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.ApiClient.Models;

namespace Infrastructure.Services;

public class ExternalDoctorService(IApiService apiService) : IExternalDoctorService
{
    public async Task<List<Doctor>> GetBySpecialtyAsync(int lpuId, string specialtyId)
    {
        var response =
            await apiService.GetAsync<List<Doctor>>(GorzdravApiEndpoints.DoctorsBySpecialty(lpuId, specialtyId));

        if (!response.Success)
            throw new HttpRequestException($"Ошибка при получении списка докторов: {response.Message}");

        return response.Result!;
    }
}