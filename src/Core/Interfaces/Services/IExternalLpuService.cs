using Core.Models;

namespace Core.Interfaces.Services;

public interface IExternalLpuService
{
    Task<List<Lpu>> GetByDistrictAsync(string districtId);
}