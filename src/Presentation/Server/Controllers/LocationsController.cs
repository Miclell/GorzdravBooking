using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using Server.Configurations.Common;

namespace Server.Controllers;

[ApiController]
[Route($"{ApiRoutes.ApiV1Prefix}/[controller]")]
public class LocationsController(
    IExternalDistrictService districtService,
    IExternalLpuService lpuService,
    ILogger<LocationsController> logger) : ControllerBase
{
    [HttpGet("districts")]
    public async Task<ActionResult<List<District>>> GetDistricts()
    {
        return await districtService.GetDistrictsAsync();
    }

    [HttpGet("district/{districtId}/lpus")]
    public async Task<ActionResult<List<Lpu>>> GetLpusByDistrict(string districtId)
    {
        return await lpuService.GetByDistrictAsync(districtId);
    }
}