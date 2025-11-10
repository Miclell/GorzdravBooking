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
    public async Task<IActionResult> Create([FromBody] CreateAppointmentSearchRequestDto request)
    {
        var result = await appointmentSearchRequestService.CreateAsync(request);

        if (result.IsSuccess)
            return Ok(result.Value);
        
        return BadRequest(result.Error);
    }
}