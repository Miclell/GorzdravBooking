using Core.Interfaces.ApiClient;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.ApiClient.Models;

namespace Infrastructure.Services;

public class ExternalAppointmentService(IApiService apiService) : IExternalAppointmentService
{
    public async Task<List<Appointment>> GetByDoctorAsync(int lpuId, string doctorId)
    {
        var response =
            await apiService.GetAsync<List<Appointment>>(GorzdravApiEndpoints.AppointmentsByDoctor(lpuId, doctorId));

        if (!response.Success)
            throw new HttpRequestException($"Ошибка при получении номерков: {response.Message}");

        return response.Result!;
    }

    public async Task<bool> CreateAppointmentAsync(AppointmentCreateRequest request)
    {
        var response =
            await apiService.PostAsync<AppointmentCreateRequest, bool>(GorzdravApiEndpoints.AppointmentCreate, request);

        return !response.Success
            ? throw new HttpRequestException($"Ошибка при выполнении записи: {response.Message}")
            : response.Result!;
    }

    public async Task<bool> CancelAppointmentAsync(AppointmentСancelRequest request)
    {
        var response =
            await apiService.PostAsync<AppointmentСancelRequest, bool>(GorzdravApiEndpoints.AppointmentCancel, request);

        if (!response.Success)
            throw new HttpRequestException($"Ошибка при отмене записи: {response.Message}");

        return response.Result!;
    }
}