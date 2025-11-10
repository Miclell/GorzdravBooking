using Application.DTOs.TimePreferences;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Server.Configurations.Common;

namespace Server.Controllers;

[ApiController]
[Route($"{ApiRoutes.ApiV1Prefix}/[controller]")]
public class TimePreferencesController(
    ITimePreferencesService timePreferencesService,
    ILogger<TimePreferencesController> logger) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] IEnumerable<CreateTimePreferenceDto> request)
    {
        var result = await timePreferencesService.CreateRangeAsync(request);
        
        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }
}