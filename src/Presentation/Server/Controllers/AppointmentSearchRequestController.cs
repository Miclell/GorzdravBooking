using Application.DTOs.AppointmentSearchRequest;
using Application.Services.Interfaces;
using Core.Entities;
using Core.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Auth;
using Server.Configurations.Common;

namespace Server.Controllers;

[ApiController]
[Authorize]
[RequireUserClaims]
[Route($"{ApiRoutes.ApiV1Prefix}/[controller]")]
public class AppointmentSearchRequestController(
    IAppointmentSearchRequestService appointmentSearchRequestService,
    IAuthorizationProvider authorizationProvider,
    ILogger<AppointmentSearchRequestController> logger) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateAppointmentSearchRequestDto request)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        var canAccess = await authorizationProvider
            .CanAccessAsync<PatientProfile>(userId, request.PatientProfileId);

        if (!canAccess)
            return Forbid();

        var result = await appointmentSearchRequestService.CreateAsync(request);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentSearchRequestDto>>> Get()
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        var result = await appointmentSearchRequestService.GetActiveByUserAsync(userId);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    [HttpPatch("update")]
    public async Task<IActionResult> Update([FromBody] UpdatePreferencesDto request)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        var canAccess = await authorizationProvider
            .CanAccessAsync<AppointmentSearchRequest>(userId, request.RequestId);

        if (!canAccess)
            return Forbid();

        var result = await appointmentSearchRequestService.UpdateTimePreferencesAsync(request);

        if (result.IsSuccess)
            return NoContent();

        return BadRequest(result.Error);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] Guid requestId)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        var canAccess = await authorizationProvider
            .CanAccessAsync<AppointmentSearchRequest>(userId, requestId);

        if (!canAccess)
            return Forbid();

        var result = await appointmentSearchRequestService.DeleteAsync(requestId);

        if (result.IsSuccess)
            return NoContent();

        return BadRequest(result.Error);
    }
}