using Serilog;

namespace Server.Configurations;

public static class SerilogConfiguration
{
    public static IHostBuilder AddSerilog(this IHostBuilder host)
    {
        return host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();

            var endpoint = context.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
            if (!string.IsNullOrWhiteSpace(endpoint))
                configuration.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = endpoint;
                    options.ResourceAttributes.Add("service.name", context.HostingEnvironment.ApplicationName);
                });
        });
    }
}
