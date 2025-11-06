using Core.Models;

namespace Core.Interfaces.Services;

public interface IExternalAppointmentService
{
    Task<List<Appointment>> GetByDoctorAsync(int lpuId, string doctorId);
    Task<(bool IsSucces, int ErrorCode)> CreateAppointmentAsync(AppointmentCreateRequest request);
    Task<bool> CancelAppointmentAsync(AppointmentСancelRequest request);
}