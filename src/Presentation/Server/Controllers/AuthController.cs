using System.Security.Claims;
using Application.DTOs.User;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Auth;
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

        if (!result.IsSuccess)
            return Unauthorized(result.Error);

        await SignInUserAsync(request.Username, result.Value);

        return new UserResponse(request.Username, result.Value);
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register([FromBody] BaseUserDto request)
    {
        var result = await userService.Create(request);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        await SignInUserAsync(request.Username, result.Value);

        return new UserResponse(request.Username, result.Value);
    }

    [Authorize]
    [RequireUserClaims]
    [HttpGet("me")]
    public ActionResult<UserResponse> GetCurrentUser()
    {
        var userId = (Guid)HttpContext.Items["UserId"]!;
        var username = (string)HttpContext.Items["Username"]!;

        return new UserResponse(username, userId);
    }

    private async Task SignInUserAsync(string username, Guid userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username)
        };

        var claimsIdentity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        logger.LogInformation("User {Username} signed in successfully", username);
    }
}