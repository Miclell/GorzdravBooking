using System.Security.Claims;

namespace Server.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        return Guid.TryParse(
            principal.FindFirst(
                ClaimTypes.NameIdentifier)?.Value, out var userId)
            ? userId
            : null;
    }

    public static string? GetUsername(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Name)?.Value;
    }
}