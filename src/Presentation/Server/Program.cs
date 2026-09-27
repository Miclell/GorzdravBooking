using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Server.Configurations;

namespace Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.ConfigureLogging();
        builder.Services.ConfigureApi(builder.Configuration);
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowReact", policy =>
            {
                policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? ["http://localhost:5173"])
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        var app = builder.Build();

        // Swagger
        app.UseSwaggerWithUi();

        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("AllowReact");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.Services.MigrateDatabaseAsync();

        await app.RunAsync();
    }
}
