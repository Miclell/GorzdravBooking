using Application.DTOs.User;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(
    IUserService userService,
    ILogger<UserController> logger) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] BaseUserDto request)
    {
        var result = await userService.Create(request);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(result.Error);
    }

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

// DTO для смены пароля TODO добавить в Application.DTOs.User
public record UpdatePasswordRequest(string NewPassword);