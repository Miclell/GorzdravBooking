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
                policy.WithOrigins("http://localhost:5173") // Vite порт
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        var app = builder.Build();

        // Swagger
        app.UseSwaggerWithUi();

        app.UseHttpsRedirection();
        app.MapControllers();

        app.UseCors("AllowReact");
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        // Migrations TODO вынести
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        await app.RunAsync();
    }
}