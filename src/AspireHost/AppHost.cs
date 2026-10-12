using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var dataDirectory = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "../../data"));
Directory.CreateDirectory(dataDirectory);

var server = builder.AddProject<Server>("server", "https")
    .WithEndpoint("http", endpoint => endpoint.Port = 5090)
    .WithEndpoint("https", endpoint => endpoint.Port = 5091)
    .WithEnvironment("ConnectionStrings__GorzdravBooking", builder.Configuration.GetConnectionString("GorzdravBooking")
                                                           ??
                                                           $"Data Source={Path.Combine(dataDirectory, "GorzdravBooking.db")}")
    .WithEnvironment("Authentication__AllowInsecureLocalhost", "true")
    .WithHttpHealthCheck("/health");

var frontend = builder.AddViteApp("frontend", "../Presentation/Web/gorzdrab-booking")
    .WithEndpoint("http", endpoint =>
    {
        endpoint.Port = 5093;
        endpoint.TargetPort = 5094;
    })
    .WithEnvironment("PORT", "5094")
    .WithArgs(context =>
    {
        // Vite reads PORT itself; remove AddViteApp's generated CLI override.
        var portArgument = context.Args.IndexOf("--port");
        if (portArgument >= 0)
        {
            context.Args.RemoveAt(portArgument + 1);
            context.Args.RemoveAt(portArgument);
            if (portArgument > 0 && Equals(context.Args[portArgument - 1], "--"))
                context.Args.RemoveAt(portArgument - 1);
        }
    })
    .WithEnvironment("API_TARGET", server.GetEndpoint("http"))
    .WithEnvironment("VITE_API_BASE_URL", "/")
    .WaitFor(server);

server.WithEnvironment("Cors__AllowedOrigins__0", frontend.GetEndpoint("http"));

builder.Build().Run();
