using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public static class DatabaseInitialization
{
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database;
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
        var knownMigrations = database.GetMigrations().ToHashSet(StringComparer.Ordinal);
        var appliedMigrations = await database.GetAppliedMigrationsAsync(cancellationToken);
        if (appliedMigrations.Any(migration => !knownMigrations.Contains(migration)))
            throw new InvalidOperationException(
                "The SQLite database has an incompatible migration history. Back it up and use a new database path " +
                "or migrate it with the application version that created it. The existing database has not been reset.");

        logger.LogInformation("Applying SQLite migrations to {DatabasePath}", database.GetDbConnection().DataSource);
        await database.MigrateAsync(cancellationToken);
    }
}
