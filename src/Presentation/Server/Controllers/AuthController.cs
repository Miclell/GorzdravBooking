using Application.DTOs.User;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Server.Configurations.Common;

namespace Server.Controllers;

[ApiController]
[Route($"{ApiRoutes.ApiV1Prefix}/[controller]")]
public class AuthController(
    IUserService userService,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login([FromBody] BaseUserDto request)
    {
        var result = await userService.Validate(request);
        
        if (result.IsSuccess)
            return new UserResponse(request.Username, result.Value);

        return BadRequest(result.Error);
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register([FromBody] BaseUserDto request)
    {
        var result = await userService.Create(request);

        if (result.IsSuccess)
            return new UserResponse(request.Username, result.Value);

        return BadRequest(result.Error);
    }
}