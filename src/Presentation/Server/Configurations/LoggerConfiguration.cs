namespace Server.Configurations;

public static class LoggerConfiguration
{
    public static void ConfigureLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                AddDebugLogging(logging);
                //AddProductionLogging(logging);
            });
    }
    
    private static void AddDebugLogging(ILoggingBuilder logging)
    {
        logging.AddConsole(_ => { })
            .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
            .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning)
            .AddFilter("System.Net.Http", LogLevel.Warning)
            .AddFilter("Default", LogLevel.Information)
            .AddFilter("Infrastructure", LogLevel.Debug)
            .AddFilter("Core", LogLevel.Debug)
            .AddFilter("Application", LogLevel.Debug)
            .AddFilter("CLI", LogLevel.Debug);
    }

    private static void AddProductionLogging(ILoggingBuilder logging)
    {
        logging.AddConsole(_ => { });

        logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Update", LogLevel.None);

        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.None);
        logging.AddFilter("Microsoft.AspNetCore.Routing", LogLevel.None);

        logging.AddFilter("System.Net.Http.HttpClient", LogLevel.None);
        logging.AddFilter("System.Net.Http.HttpClient.*", LogLevel.None);

        logging.AddFilter("Default", LogLevel.Information);
        logging.AddFilter("Infrastructure", LogLevel.Information);
        logging.AddFilter("Core", LogLevel.Information);
        logging.AddFilter("Application", LogLevel.Information);
        logging.AddFilter("CLI", LogLevel.Information);
    }
}