using Core.Models;

namespace Core.Interfaces.Services;

public interface IExternalDoctorService
{
    Task<List<Doctor>> GetBySpecialtyAsync(int lpuId, string specialtyId);
}