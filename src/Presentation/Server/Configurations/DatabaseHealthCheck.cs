using Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Server.Configurations;

public sealed class DatabaseHealthCheck(AppDbContext database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return await database.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("SQLite is unavailable.");
    }
}