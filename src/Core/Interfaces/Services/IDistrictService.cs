using Core.Models;

namespace Core.Interfaces.Services;

public interface IDistrictService
{
    Task<List<District>> GetDistrictsAsync();
}