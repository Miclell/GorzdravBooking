using Application.DTOs.User;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Server.Configurations.Common;

namespace Server.Controllers;

[ApiController]
[Route($"{ApiRoutes.ApiV1Prefix}/[controller]")]
public class UserController(
    IUserService userService,
    ILogger<UserController> logger) : ControllerBase
{
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var result = await userService.Delete(userId);

        if (result.IsSuccess)
            return NoContent();

        return BadRequest(result.Error);
    }

    [HttpPut("{userId:guid}/password")]
    public async Task<IActionResult> UpdatePassword(
        Guid userId,
        [FromBody] UpdatePasswordRequest request)
    {
        var result = await userService.UpdatePassword(userId, request.NewPassword);

        if (result.IsSuccess)
            return NoContent();

        return BadRequest(result.Error);
    }
}