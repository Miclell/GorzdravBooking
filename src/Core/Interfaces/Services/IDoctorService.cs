using Core.Models;

namespace Core.Interfaces.Services;

public interface IDoctorService
{
    Task<List<Doctor>> GetBySpecialtyAsync(int lpuId, string specialtyId);
}