using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Server.Identity.Attributes;

public class ValidateCurrentUserAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var currentUserId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(currentUserId))
        {
            context.Result = new UnauthorizedObjectResult("Пользователь не авторизован");
            return;
        }

        if (context.ActionArguments.TryGetValue("userId", out var userIdObj) && 
            userIdObj is Guid requestedUserId)
        {
            if (requestedUserId.ToString() != currentUserId)
            {
                context.Result = new ForbidResult("Вы можете получать данные только для своего аккаунта");
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}