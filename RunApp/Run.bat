@echo off
cd /d "%~dp0"

echo ClinicVets — RunApp (fix branch)
echo.

dotnet run --project "ClinicVets.csproj"

if errorlevel 1 (
    echo.
    echo Could not start. Install .NET 9 SDK: https://dotnet.microsoft.com/download
    echo.
)

pause
