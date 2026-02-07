using Application.DTOs.Appointment;
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
public class AppointmentController(
    IAppointmentService appointmentService,
    IAuthorizationProvider authorizationProvider,
    ILogger<AppointmentController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AppointmentListItemDto>> Get()
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        var result = await appointmentService.GetByUserAsync(userId);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    [HttpDelete]
    public async Task<IActionResult> Cancel([FromBody] Guid appointmentId)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        var canAccess = await authorizationProvider
            .CanAccessAsync<Appointment>(userId, appointmentId);

        if (!canAccess)
            return Forbid();

        var result = await appointmentService.DeleteAsync(appointmentId);
        if (result.IsSuccess)
            return NoContent();

        return BadRequest(result.Error);
    }
}