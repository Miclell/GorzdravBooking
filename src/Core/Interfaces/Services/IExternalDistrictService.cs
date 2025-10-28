using Core.Models;

namespace Core.Interfaces.Services;

public interface IExternalDistrictService
{
    Task<List<District>> GetDistrictsAsync();
}