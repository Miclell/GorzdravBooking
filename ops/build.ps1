$ErrorActionPreference = 'Stop'
dotnet fsi (Join-Path $PSScriptRoot 'build.fsx') -- @args
exit $LASTEXITCODE
