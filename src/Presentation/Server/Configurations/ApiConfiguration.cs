using Application;
using Application.Workers;
using Infrastructure;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Server.Configurations.Common;

namespace Server.Configurations;

public static class ApiConfiguration
{
    public static void ConfigureApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureRoutes();
        services.ConfigureCookieAuthentication(configuration);
        services.ConfigureSwagger();

        services.AddInfrastructure();
        services.AddApplication();
        services.AddHostedService<AppointmentSchedulerWorker>();

        services.AddControllers(options =>
        {
            options.Conventions.Add(new RouteTokenTransformerConvention(
                new KebabCaseParameterTransformer()));
        });

        services.ConfigureFluentValidation();
    }
}