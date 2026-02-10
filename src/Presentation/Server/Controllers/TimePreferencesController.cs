using Application.DTOs.TimePreferences;
using Application.Services.Interfaces;
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
public class TimePreferencesController(
    ITimePreferencesService timePreferencesService,
    IAuthorizationProvider authorizationProvider,
    ILogger<TimePreferencesController> logger) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<List<Guid>>> Create([FromBody] List<CreateTimePreferenceDto> request)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        if (request.Any(tp => tp.UserId != userId))
            return Forbid();

        var result = await timePreferencesService.CreateRangeAsync(request);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    [HttpGet("{name}")]
    public async Task<ActionResult<TimePreferencesPresetDto>> Get(string name)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        var result = await timePreferencesService.GetByPresetAsync(userId, name);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest();
    }

    [HttpDelete]
    public async Task<ActionResult> Delete([FromBody] DeleteTimePreferencesDto request)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        if (userId != request.UserId)
            return Forbid();

        var result = await timePreferencesService.DeleteByPresetAsync(request);

        if (result.IsSuccess)
            return NoContent();

        return BadRequest();
    }
}