using Application.DTOs.User;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Auth;
using Server.Configurations.Common;

namespace Server.Controllers;

[ApiController]
[Authorize]
[RequireUserClaims]
[Route($"{ApiRoutes.ApiV1Prefix}/[controller]")]
public class UserController(
    IUserService userService,
    ILogger<UserController> logger) : ControllerBase
{
    [HttpDelete]
    public async Task<IActionResult> DeleteUser()
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        var result = await userService.Delete(userId);

        if (result.IsSuccess)
            return NoContent();

        return BadRequest(result.Error);
    }

    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword(
        [FromBody] UpdatePasswordRequest request)
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;

        var result = await userService.UpdatePassword(userId, request.NewPassword);

        if (result.IsSuccess)
            return NoContent();

        return BadRequest(result.Error);
    }
}