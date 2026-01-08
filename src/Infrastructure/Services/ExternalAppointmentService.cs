using Core.Interfaces.ApiClient;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.Referral;
using Infrastructure.ApiClient;

namespace Infrastructure.Services;

public class ExternalAppointmentService(
    IApiService apiService,
    IExternalDoctorService externalDoctorService) : IExternalAppointmentService
{
    public async Task<List<(string Doctor, Appointment Appointment)>> GetBySpecialityAsync(int lpuId,
        string specialtyId)
    {
        var doctors = await externalDoctorService.GetBySpecialtyAsync(lpuId, specialtyId);

        var result = new List<(string Doctor, Appointment Appointment)>();
        foreach (var doctor in doctors)
        {
            var response =
                await apiService.GetAsync<List<Appointment>>(
                    GorzdravApiEndpoints.AppointmentsByDoctor(lpuId, doctor.Id));

            if (!response.Success)
                //continue;
                // TODO найти оптимальное решение
                throw new HttpRequestException($"Ошибка при получении номерков: {response.Message}");

            if (response.Result != null) result.AddRange(response.Result.Select(a => (doctor.Name, a)));
        }

        return result;
    }

    public async Task<List<Appointment>> GetByDoctorAsync(int lpuId, string doctorId)
    {
        var response =
            await apiService.GetAsync<List<Appointment>>(GorzdravApiEndpoints.AppointmentsByDoctor(lpuId, doctorId));

        if (!response.Success)
            throw new HttpRequestException($"Ошибка при получении номерков: {response.Message}");

        return response.Result!;
    }

    public async Task<List<ReferralResult>> GetByReferralAsync(int referralNumber, string lastName)
    {
        var response =
            await apiService.GetAsync<List<ReferralResult>>(
                GorzdravApiEndpoints.AppointmentsByReferral(referralNumber, lastName));

        if (!response.Success)
            throw new HttpRequestException($"Ошибка при получении номерков: {response.Message}");

        return response.Result!;
    }

    public async Task<(bool IsSucces, int ErrorCode)> CreateAppointmentAsync(AppointmentCreateRequest request)
    {
        var response =
            await apiService.PostAsync<AppointmentCreateRequest, bool>(GorzdravApiEndpoints.AppointmentCreate, request);

        return !response.Success
            ? throw new HttpRequestException($"Ошибка при выполнении записи: {response.Message}")
            : (response.Success, response.ErrorCode);
    }

    public async Task<bool> CancelAppointmentAsync(AppointmentСancelRequest request)
    {
        var response =
            await apiService.PostAsync<AppointmentСancelRequest, bool>(GorzdravApiEndpoints.AppointmentCancel, request);

        if (!response.Success)
            throw new HttpRequestException($"Ошибка при отмене записи: {response.Message}");

        return response.Result!;
    }

    public async Task<(bool IsSucces, int ErrorCode)> CreateReferralAppointmentAsync(AppointmentCreateRequest request)
    {
        throw new NotImplementedException();
    }
}