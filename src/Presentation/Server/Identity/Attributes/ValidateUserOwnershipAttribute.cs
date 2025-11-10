using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Server.Identity.Attributes;

public class ValidateUserOwnershipAttribute(
    string idParameterName = "id",
    Type serviceType = null,
    string methodName = null)
    : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var currentUserId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(currentUserId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Проверка владения ресурсом через сервис
        if (serviceType != null && !string.IsNullOrEmpty(methodName))
        {
            if (await CheckOwnershipViaService(context, currentUserId))
            {
                await next();
                return;
            }
        }

        // Базовая проверка по userId параметру
        if (context.ActionArguments.TryGetValue(idParameterName, out var idObj))
        {
            if (idObj is Guid id && id.ToString() == currentUserId)
            {
                await next();
                return;
            }
        }

        context.Result = new ForbidResult("Доступ запрещен");
    }

    private async Task<bool> CheckOwnershipViaService(ActionExecutingContext context, string currentUserId)
    {
        try
        {
            var service = context.HttpContext.RequestServices.GetService(serviceType);
            if (service == null) return false;

            var method = serviceType.GetMethod(methodName);
            if (method == null) return false;

            // Получаем ID сущности из параметров
            if (!context.ActionArguments.TryGetValue(idParameterName, out var entityIdObj))
                return false;

            // Вызываем метод сервиса для проверки владения
            var parameters = new[] { entityIdObj, Guid.Parse(currentUserId) };
            var result = method.Invoke(service, parameters);
            
            if (result is Task<bool> task)
            {
                return await task;
            }
            
            return result as bool? ?? false;
        }
        catch
        {
            return false;
        }
    }
}