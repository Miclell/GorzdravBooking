@echo off
dotnet fsi "%~dp0add-migration.fsx" -- %*
exit /b %errorlevel%
