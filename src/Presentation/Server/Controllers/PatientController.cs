using Application.DTOs.Patient;
using Application.Services.Interfaces;
using Core.Entities;
using Core.Interfaces.Auth;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Auth;
using Server.Configurations.Common;

namespace Server.Controllers;

[ApiController]
[Route($"{ApiRoutes.ApiV1Prefix}/[controller]")]
public class PatientController(
    IPatientService patientService,
    IExternalPatientService externalPatientService,
    IAuthorizationProvider authorizationProvider,
    ILogger<PatientController> logger) : ControllerBase
{
    [Authorize]
    [RequireUserClaims]
    [HttpPost("create")]
    public async Task<ActionResult<Guid>> Create([FromBody] CreatePatientDto request)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        if (userId != request.UserId)
            return Forbid();

        var result = await patientService.Create(request);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    [HttpPost("find")]
    public async Task<ActionResult<string>> Find([FromBody] PatientIdSearchRequest request)
    {
        return await externalPatientService.GetPatientIdAsync(request);
    }

    [Authorize]
    [RequireUserClaims]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BasePatientProfileDto>>> Get()
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        var result = await patientService.GetByUser(userId);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    [Authorize]
    [RequireUserClaims]
    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] Guid patientId)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        var canAccess = await authorizationProvider
            .CanAccessAsync<PatientProfile>(userId, patientId);

        if (!canAccess)
            return Forbid();

        var result = await patientService.Delete(patientId);
        if (result.IsSuccess)
            return NoContent();

        return BadRequest(result.Error);
    }
}