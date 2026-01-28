using Core.Models;
using Core.Models.Referral;

namespace Core.Interfaces.Services;

public interface IExternalAppointmentService
{
    Task<List<(string Doctor, Appointment Appointment)>> GetBySpecialityAsync(int lpuId, string specialityId);
    Task<List<Appointment>> GetByDoctorAsync(int lpuId, string doctorId);
    Task<ReferralResult> GetByReferralAsync(string referralNumber, string lastName);
    Task<(bool IsSucces, int ErrorCode)> CreateAppointmentAsync(AppointmentCreateRequest request);
    Task<bool> CancelAppointmentAsync(AppointmentСancelRequest request);
}