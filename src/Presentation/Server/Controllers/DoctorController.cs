using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using Server.Configurations.Common;

namespace Server.Controllers;

[ApiController]
[Route($"{ApiRoutes.ApiV1Prefix}/[controller]")]
public class DoctorController(
    IExternalDoctorService doctorService,
    ILogger<DoctorController> logger) : ControllerBase
{
    [HttpGet("{lpuId:int}/{specialityId}")]
    public async Task<ActionResult<List<Doctor>>> GetBySpeciality(int lpuId, string specialityId) =>
        await doctorService.GetBySpecialtyAsync(lpuId, specialityId);
}