using Application.DTOs.Appointment;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Server.Configurations.Common;

namespace Server.Controllers;

[ApiController]
[Route($"{ApiRoutes.ApiV1Prefix}/[controller]")]
public class AppointmentController(
    IAppointmentService appointmentService,
    ILogger<AppointmentController> logger) : ControllerBase
{
    [HttpGet("get/{userId:Guid}")]
    public async Task<ActionResult<AppointmentListItemDto>> Get(Guid userId)
    {
        var result = await appointmentService.GetByUserAsync(userId);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    [HttpDelete]
    public async Task<IActionResult> Cancel([FromBody] Guid appointmentId)
    {
        var result = await appointmentService.DeleteAsync(appointmentId);

        if (result.IsSuccess)
            return NoContent();

        return BadRequest();
    }
}