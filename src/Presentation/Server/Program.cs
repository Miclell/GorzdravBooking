using System.Globalization;
using Infrastructure.Persistence;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Server.Configurations;
using ServiceDefaults;

namespace Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(
                theme: AnsiConsoleTheme.Sixteen,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {Message:lj}{NewLine}{Exception}",
                formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                ContentRootPath = AppContext.BaseDirectory
            });

            builder.AddServiceDefaults();
            builder.Host.AddSerilog();
            builder.Services.ConfigureApi(builder.Configuration);
            builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");
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

            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (context, _, exception) =>
                {
                    if (context.Request.Path.StartsWithSegments("/health") ||
                        context.Request.Path.StartsWithSegments("/alive"))
                        return LogEventLevel.Verbose;

                    return exception is not null || context.Response.StatusCode >= 500
                        ? LogEventLevel.Error
                        : LogEventLevel.Information;
                };
            });

            app.UseSwaggerWithUi();

            if (!app.Environment.IsDevelopment())
                app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowReact");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapDefaultEndpoints();
            await app.Services.MigrateDatabaseAsync();

            await app.RunAsync();
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Server terminated unexpectedly");
            throw;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
