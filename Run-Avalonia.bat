@echo off
cd /d "%~dp0"

echo Starting ClinicVets Avalonia (Hebrew UI at repo root)...
echo.

dotnet run --project "ClinicVetsAvalonia.csproj"

if errorlevel 1 (
    echo.
    echo The app could not start.
    echo Make sure the .NET SDK is installed on this computer.
    echo Download: https://dotnet.microsoft.com/download
    echo.
)

pause
