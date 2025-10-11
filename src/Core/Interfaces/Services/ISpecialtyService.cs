using Core.Models;

namespace Core.Interfaces.Services;

public interface ISpecialtyService
{
    Task<List<MedicalSpeciality>> GetByLpuAsync(int lpuId);
}