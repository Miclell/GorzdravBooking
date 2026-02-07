using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Server.Auth;

public class RequireUserClaimsAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var userId = context.HttpContext.User.GetUserId();
        var username = context.HttpContext.User.GetUsername();

        if (userId == null || username == null)
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<RequireUserClaimsAttribute>>();

            logger.LogWarning("Invalid claims for authenticated user");

            context.Result = new UnauthorizedResult();
            return;
        }

        context.HttpContext.Items["UserId"] = userId.Value;
        context.HttpContext.Items["Username"] = username;
    }
}