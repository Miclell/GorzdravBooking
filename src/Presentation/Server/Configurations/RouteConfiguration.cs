using Server.Configurations.Common;

namespace Server.Configurations;

public static class RouteConfiguration
{
    public static void ConfigureRoutes(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = false;
        });

        services.AddSingleton<IOutboundParameterTransformer, KebabCaseParameterTransformer>();
    }
}