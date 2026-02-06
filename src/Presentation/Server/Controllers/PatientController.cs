using Application.DTOs.Patient;
using Application.Services.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using Server.Configurations.Common;

namespace Server.Controllers;

[ApiController]
[Route($"{ApiRoutes.ApiV1Prefix}/[controller]")]
public class PatientController(
    IPatientService patientService,
    IExternalPatientService externalPatientService,
    ILogger<PatientController> logger) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<Guid>> Create([FromBody] CreatePatientDto request)
    {
        var result = await patientService.Create(request);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    [HttpPost("find")]
    public async Task<ActionResult<string>> Find([FromBody] PatientIdSearchRequest request) =>
        await externalPatientService.GetPatientIdAsync(request);

    [HttpGet("get/{userId:Guid}")]
    public async Task<ActionResult<IEnumerable<BasePatientProfileDto>>> Get(Guid userId)
    {
        var result = await patientService.GetByUser(userId);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] Guid patientId)
    {
        var result = await patientService.Delete(patientId);

        if (result.IsSuccess)
            return NoContent();

        return BadRequest();
    }
}