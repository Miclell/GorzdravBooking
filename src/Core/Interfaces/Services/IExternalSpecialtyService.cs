using Core.Models;

namespace Core.Interfaces.Services;

public interface IExternalSpecialtyService
{
    Task<List<MedicalSpeciality>> GetByLpuAsync(int lpuId);
}