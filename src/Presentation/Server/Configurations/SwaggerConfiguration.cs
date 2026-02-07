using Microsoft.OpenApi.Models;

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

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Cookie"
                        }
                    },
                    Array.Empty<string>()
                }
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