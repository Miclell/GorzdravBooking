#!/usr/bin/env bash
set -euo pipefail
cd "$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
dotnet run --project ./_build/_build.csproj --configuration Release -- "$@"
