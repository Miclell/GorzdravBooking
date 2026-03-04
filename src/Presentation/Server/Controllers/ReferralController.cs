using Application.DTOs.UseCases;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Auth;
using Server.Configurations.Common;

namespace Server.Controllers;

[ApiController]
[Authorize]
[RequireUserClaims]
[Route($"{ApiRoutes.ApiV1Prefix}/[controller]")]
public class ReferralController(ProcessReferralUseCase processReferralUseCase) : ControllerBase
{
    [HttpPost("validate")]
    public async Task<ActionResult<ReferralValidationResult>> Validate([FromBody] ValidateReferralDto request)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        var result = await processReferralUseCase.Execute(
            new ReferralValidationRequest(userId, request.ReferralNumber, request.LastName));

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }
}
