using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var dataDirectory = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "../../data"));
Directory.CreateDirectory(dataDirectory);

var server = builder.AddProject<Server>("server", "http")
    .WithEnvironment("ConnectionStrings__GorzdravBooking", builder.Configuration.GetConnectionString("GorzdravBooking")
                                                           ??
                                                           $"Data Source={Path.Combine(dataDirectory, "GorzdravBooking.db")}")
    .WithEnvironment("Authentication__AllowInsecureLocalhost", "true")
    .WithHttpHealthCheck("/health");

var frontend = builder.AddViteApp("frontend", "../Presentation/Web/gorzdrab-booking")
    .WithEnvironment("API_TARGET", server.GetEndpoint("http"))
    .WithEnvironment("VITE_API_BASE_URL", "/")
    .WaitFor(server);

server.WithEnvironment("Cors__AllowedOrigins__0", frontend.GetEndpoint("http"));

builder.Build().Run();
