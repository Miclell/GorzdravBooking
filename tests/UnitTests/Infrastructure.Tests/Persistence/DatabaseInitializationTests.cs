using Core.Entities;
using Core.Interfaces.ApiClient;
using Infrastructure.Persistence;
using Infrastructure.Stubs;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Infrastructure.Tests.Persistence;

public sealed class DatabaseInitializationTests : IDisposable
{
    private readonly string _directory =
        Path.Combine(Path.GetTempPath(), "GorzdravBooking.Tests", Guid.NewGuid().ToString("N"));

    private ServiceProvider CreateServices()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:GorzdravBooking"] = $"Data Source={Path.Combine(_directory, "test.db")};Pooling=False"
        }).Build();
        var services = new ServiceCollection().AddSingleton<IConfiguration>(configuration)
            .AddLogging().AddInfrastructure();
        services.RemoveAll<IApiService>();
        services.TryAddSingleton<FakeApiDataService>();
        services.AddScoped<IApiService, FakeApiService>();
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    [Fact]
    public async Task Startup_CreatesSchema_AndSecondStartupPreservesData()
    {
        await using (var services = CreateServices())
        {
            await services.MigrateDatabaseAsync();
            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Single(await db.Database.GetAppliedMigrationsAsync());
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.IsType<FakeApiService>(scope.ServiceProvider.GetRequiredService<IApiService>());
            db.AppSettings.Add(new AppSetting { Key = "bootstrap-test", Value = "preserved" });
            await db.SaveChangesAsync();
        }

        await using (var services = CreateServices())
        {
            await services.MigrateDatabaseAsync();
            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Single(await db.Database.GetAppliedMigrationsAsync());
            Assert.Equal("preserved", (await db.AppSettings.SingleAsync()).Value);
        }
    }

    [Fact]
    public async Task Startup_RejectsUnknownHistory_WithoutDeletingData()
    {
        await using var services = CreateServices();
        await services.MigrateDatabaseAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20000101000000_Legacy', '9.0.8')");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => services.MigrateDatabaseAsync());
        Assert.Contains("incompatible migration history", exception.Message);
        Assert.Equal(2, (await db.Database.GetAppliedMigrationsAsync()).Count());
    }

    [Fact]
    public void RelativePath_IsAbsolute_AndSharedWithDesignTimeFactory()
    {
        var configuration = new ConfigurationBuilder().Build();
        var connection = new SqliteConnectionStringBuilder(DatabaseConfiguration.GetConnectionString(configuration));
        Assert.True(Path.IsPathFullyQualified(connection.DataSource));
        using var designTime = new DesignTimeDbContextFactory().CreateDbContext([]);
        Assert.Equal(connection.ToString(), designTime.Database.GetConnectionString());
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, true);
    }
}
