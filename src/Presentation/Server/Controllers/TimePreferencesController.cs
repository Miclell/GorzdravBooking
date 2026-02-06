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
    public async Task<ActionResult<List<Guid>>> Create([FromBody] IEnumerable<CreateTimePreferenceDto> request)
    {
        var result = await timePreferencesService.CreateRangeAsync(request);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

    [HttpGet("{userId:guid}/{name}")]
    public async Task<ActionResult<TimePreferencesPresetDto>> Get(Guid userId, string name)
    {
        var result = await  timePreferencesService.GetByPresetAsync(userId, name);
        
        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest();
    }
    
    [HttpDelete]
    public async Task<ActionResult> Delete([FromBody] DeleteTimePreferencesDto request)
    {
        var result = await timePreferencesService.DeleteByPresetAsync(request);
        
        if (result.IsSuccess)
            return NoContent();

        return BadRequest();
    }
}