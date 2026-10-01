#load "common.fsx"

let arguments = fsi.CommandLineArgs |> Array.skip 1 |> Array.toList

match arguments with
| [] ->
    eprintfn "Usage: dotnet fsi scripts/add-migration.fsx -- <MigrationName> [NUKE options]"
    exit 1
| [ "--help" ] | [ "-h" ] ->
    printfn "Usage: dotnet fsi scripts/add-migration.fsx -- <MigrationName> [NUKE options]"
| name :: options ->
    if System.String.IsNullOrWhiteSpace(name) || name.StartsWith("-") then
        eprintfn "Specify a migration name as the first argument."
        exit 1
    Helper.runDotnet (
        [ "run"; "--project"; "_build/_build.csproj"; "--configuration"; "Release"; "--"
          "--target"; "AddMigration"; "--migration-name"; name ] @ options)
