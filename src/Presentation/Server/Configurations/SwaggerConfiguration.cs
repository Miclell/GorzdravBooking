using Microsoft.OpenApi;

namespace Server.Configurations;

public static class SwaggerConfiguration
{
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "GorzravBooking API", Version = "v1" });

            c.AddSecurityDefinition("Cookie", new OpenApiSecurityScheme
            {
                Description = "Cookie authentication using GorzdravBooking.Cookie",
                Name = "GorzdravBooking.Cookie",
                In = ParameterLocation.Cookie,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Cookie"
            });

            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Cookie", document)] = []
            });
        });
    }

    public static void UseSwaggerWithUi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        app.UseSwagger();
        app.UseSwaggerUI(options => { options.EnablePersistAuthorization(); });
    }
}
