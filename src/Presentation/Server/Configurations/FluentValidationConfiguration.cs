using FluentValidation.AspNetCore;
using Server.Validators.User;

namespace Server.Configurations;

public static class FluentValidationConfiguration
{
    [Obsolete("Obsolete")]
    public static void ConfigureFluentValidation(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddFluentValidation(fv => { fv.RegisterValidatorsFromAssemblyContaining<BaseUserDtoValidator>(); });
    }
}