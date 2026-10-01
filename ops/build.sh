#!/usr/bin/env sh
exec dotnet fsi "$(dirname "$0")/build.fsx" -- "$@"
