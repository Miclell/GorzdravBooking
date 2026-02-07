using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using Server.Configurations.Common;

namespace Server.Controllers;

[ApiController]
[Route($"{ApiRoutes.ApiV1Prefix}/[controller]")]
public class SpecialityController(
    IExternalSpecialtyService specialtyService,
    ILogger<SpecialityController> logger) : ControllerBase
{
    [HttpGet("{lpuId:int}")]
    public async Task<ActionResult<List<MedicalSpeciality>>> GetSpecialitiesByLpu(int lpuId)
    {
        return await specialtyService.GetByLpuAsync(lpuId);
    }
}