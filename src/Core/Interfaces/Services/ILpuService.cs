using Core.Models;

namespace Core.Interfaces.Services;

public interface ILpuService
{
    Task<List<Lpu>> GetByDistrictAsync(string districtId);
}