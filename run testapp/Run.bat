@echo off
cd /d "%~dp0"

echo ClinicVets (official app — run testapp folder)
echo.

dotnet run --project "ClinicVetsAvalonia.csproj"

if errorlevel 1 (
    echo.
    echo Could not start. Install .NET 9 SDK: https://dotnet.microsoft.com/download
    echo.
)

pause
