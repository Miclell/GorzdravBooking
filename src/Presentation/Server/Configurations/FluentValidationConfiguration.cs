using FluentValidation;
using FluentValidation.AspNetCore;
using Server.Validators.User;

namespace Server.Configurations;

public static class FluentValidationConfiguration
{
    public static void ConfigureFluentValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<BaseUserDtoValidator>();
    }
}