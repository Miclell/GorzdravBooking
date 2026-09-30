using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

public static class DatabaseConfiguration
{
    public static string GetConnectionString(IConfiguration configuration)
    {
        var connection = new SqliteConnectionStringBuilder(
            configuration.GetConnectionString("GorzdravBooking") ?? "Data Source=GorzdravBooking.db");
        if (connection.DataSource == ":memory:" || connection.Mode == SqliteOpenMode.Memory)
            return connection.ToString();
        if (string.IsNullOrWhiteSpace(connection.DataSource))
            throw new InvalidOperationException("ConnectionStrings:GorzdravBooking must specify a SQLite Data Source.");

        connection.DataSource = Path.GetFullPath(connection.DataSource, GetDataDirectory());
        Directory.CreateDirectory(Path.GetDirectoryName(connection.DataSource)!);
        return connection.ToString();
    }

    private static string GetDataDirectory()
    {
        // Source runs share one database; published apps keep it beside the executable.
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "GorzdravBooking.slnx")))
                return Path.Combine(directory.FullName, "data");

        return AppContext.BaseDirectory;
    }
}
