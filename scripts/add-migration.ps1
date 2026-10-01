$ErrorActionPreference = 'Stop'
dotnet fsi (Join-Path $PSScriptRoot 'add-migration.fsx') -- @args
exit $LASTEXITCODE
