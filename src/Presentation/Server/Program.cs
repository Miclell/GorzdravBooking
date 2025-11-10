using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Server.Configurations;

namespace Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.ConfigureApi(builder.Configuration);

        var app = builder.Build();

        // Swagger
        app.UseSwaggerWithUI();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // Migrations TODO вынести
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        await app.RunAsync();
    }
}