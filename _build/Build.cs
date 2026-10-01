using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.EntityFramework;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.EntityFramework.EntityFrameworkTasks;

internal class BuildPipeline : NukeBuild
{
    [Parameter("Build configuration (Debug or Release).")]
    private readonly string Configuration = "Release";

    [Parameter("CLI runtime identifier (for example, linux-x64). Defaults to the current platform.")]
    private readonly string? Runtime = null;

    [Parameter("Name of the EF Core migration to create.")]
    private readonly string? MigrationName = null;

    private static AbsolutePath SolutionFile => RootDirectory / "GorzdravBooking.slnx";
    private static AbsolutePath Artifacts => RootDirectory / "artifacts";
    private static AbsolutePath FrontendDirectory => RootDirectory / "src/Presentation/Web/gorzdrab-booking";

    private string PublishRuntime
    {
        get
        {
            if (Runtime is not null)
                return ValidateRuntime(Runtime);

            if (OperatingSystem.IsAndroid() ||
                RuntimeInformation.RuntimeIdentifier.StartsWith("linux-bionic-", StringComparison.Ordinal))
                throw new PlatformNotSupportedException(
                    "Termux uses Android bionic; this self-contained single-file CLI target does not support it. Build and run with the Termux .NET SDK instead.");

            var architecture = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
            if (architecture is not ("x64" or "arm64"))
                throw new PlatformNotSupportedException("Use --runtime to select a supported x64 or arm64 target.");

            var system = OperatingSystem.IsWindows()
                ? "win"
                : OperatingSystem.IsMacOS()
                    ? "osx"
                    : OperatingSystem.IsLinux() &&
                      RuntimeInformation.RuntimeIdentifier.StartsWith("linux-musl-", StringComparison.Ordinal)
                        ? "linux-musl"
                        : OperatingSystem.IsLinux()
                            ? "linux"
                            : throw new PlatformNotSupportedException("Use --runtime to select a supported target.");

            return ValidateRuntime($"{system}-{architecture}");
        }
    }

    private Target Restore => target => target.Executes(() =>
        DotNetRestore(settings => settings.SetProjectFile(SolutionFile)));

    private Target Build => target => target.DependsOn(Restore).Executes(() =>
        DotNetBuild(settings => settings.SetProjectFile(SolutionFile)
            .SetConfiguration(Configuration).EnableNoRestore()));

    private Target Test => target => target.Executes(() =>
    {
        var projects = (RootDirectory / "tests/UnitTests").GlobFiles("**/*.csproj");
        if (projects.Count == 0)
            throw new InvalidOperationException("No unit test projects found in tests/UnitTests.");

        foreach (var project in projects)
            DotNetTest(settings => settings.SetProjectFile(project).SetConfiguration(Configuration));
    });

    [UsedImplicitly]
    private Target AddMigration => target => target
        .Requires(() => MigrationName)
        .Executes(() =>
        {
            if (string.IsNullOrWhiteSpace(MigrationName) || MigrationName.StartsWith('-'))
                throw new ArgumentException("Specify --migration-name with a non-empty migration name.");

            DotNet("tool restore", RootDirectory);
            var project = RootDirectory / "src/Infrastructure/Infrastructure.csproj";
            EntityFrameworkMigrationsAdd(settings => settings
                .SetName(MigrationName)
                .SetProject(project)
                .SetStartupProject(project)
                .SetContext("AppDbContext")
                .SetOutputDirectory("Persistence/Migrations")
                .SetConfiguration(Configuration)
                .SetProcessWorkingDirectory(RootDirectory));
        });

    private Target Slopwatch => target => target.Executes(() =>
    {
        DotNet("tool restore", RootDirectory);
        DotNet("slopwatch analyze --no-baseline --fail-on warning", RootDirectory);
    });

    [UsedImplicitly]
    private Target Format => target => target.DependsOn(Restore).Executes(() =>
        DotNet($"format \"{SolutionFile}\" --verify-no-changes --no-restore", RootDirectory));

    private Target Frontend => target => target.Executes(() =>
    {
        var npm = ToolResolver.GetPathTool("npm");
        npm("ci", FrontendDirectory);
        npm("run build", FrontendDirectory);
    });

    [UsedImplicitly] private Target Check => target => target.DependsOn(Test, Slopwatch, Frontend);

    [UsedImplicitly]
    private Target CLI => target => target.Executes(() =>
    {
        var runtime = PublishRuntime;
        DotNetPublish(settings => settings
            .SetProject(RootDirectory / "src/Presentation/CLI/CLI.csproj")
            .SetConfiguration(Configuration).SetRuntime(runtime).EnableSelfContained()
            .SetOutput(Artifacts / "cli" / runtime)
            .SetProperty("PublishSingleFile", "true")
            .SetProperty("IncludeNativeLibrariesForSelfExtract", "true")
            .SetProperty("DebugType", "embedded"));
    });

    [UsedImplicitly]
    private Target Server => target => target.Executes(() =>
        DotNetPublish(settings => settings
            .SetProject(RootDirectory / "src/Presentation/Server/Server.csproj")
            .SetConfiguration(Configuration).SetOutput(Artifacts / "server")));

    [UsedImplicitly]
    private Target Docker => target => target.Executes(() =>
        ToolResolver.GetPathTool("docker")(
            "build -f src/Presentation/Server/Dockerfile -t gorzdravbooking-server:local .",
            RootDirectory));

    public static int Main()
    {
        return Execute<BuildPipeline>(build => build.Build);
    }

    private static string ValidateRuntime(string runtime)
    {
        return runtime switch
        {
            "linux-x64" or "linux-arm64" or "linux-musl-x64" or "linux-musl-arm64"
                or "win-x64" or "win-arm64" or "osx-x64" or "osx-arm64" => runtime,
            _ => throw new ArgumentException("--runtime must be a supported portable .NET RID, such as linux-x64.")
        };
    }
}
