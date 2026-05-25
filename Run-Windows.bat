@echo off
cd /d "%~dp0"

set EXE=%~dp0RunApp\ClinicVets.exe

if exist "%EXE%" (
    echo Starting ClinicVets...
    start "" "%EXE%"
    exit /b 0
)

echo RunApp\ClinicVets.exe not found.
echo Build it with: powershell -File scripts\Publish-RunApp.ps1
echo Or run from source: dotnet run --project Source\ClinicVetsAvalonia.csproj
echo.
pause
exit /b 1
