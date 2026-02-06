using Application.DTOs.AppointmentSearchRequest;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Server.Configurations.Common;

namespace Server.Controllers;

[ApiController]
[Route($"{ApiRoutes.ApiV1Prefix}/[controller]")]
public class AppointmentSearchRequestController(
    IAppointmentSearchRequestService appointmentSearchRequestService,
    ILogger<AppointmentSearchRequestController> logger) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateAppointmentSearchRequestDto request)
    {
        var result = await appointmentSearchRequestService.CreateAsync(request);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    [HttpGet("get/{userId:Guid}")]
    public async Task<ActionResult<IEnumerable<AppointmentSearchRequestDto>>> Get(Guid userId)
    {
        var result = await appointmentSearchRequestService.GetActiveByUserAsync(userId);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    [HttpPatch("update")]
    public async Task<IActionResult> Update([FromBody] UpdatePreferencesDto request)
    {
        var result = await appointmentSearchRequestService.UpdateTimePreferencesAsync(request);

        if (result.IsSuccess)
            return NoContent();
        
        return BadRequest(result.Error);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] Guid requestId)
    {
        var result = await appointmentSearchRequestService.DeleteAsync(requestId);

        if (result.IsSuccess)
            return NoContent();

        return BadRequest(result.Error);
    }
}