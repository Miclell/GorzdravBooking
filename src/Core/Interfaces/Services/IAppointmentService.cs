using Core.Models;

namespace Core.Interfaces.Services;

public interface IAppointmentService
{
    Task<List<Appointment>> GetByDoctorAsync(int lpuId, string doctorId);
    Task<bool> CreateAppointmentAsync(AppointmentCreateRequest request);
    Task<bool> CancelAppointmentAsync(AppointmentСancelRequest request);
}