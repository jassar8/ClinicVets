@echo off
cd /d "%~dp0"

echo Building desktop app (syncs to RunApp like publish)...
dotnet build "src\Frontend\ClinicVets.Desktop.csproj" -c Debug --nologo
if errorlevel 1 (
    echo.
    echo Build failed. Install the .NET 9 SDK: https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)

if not exist "RunApp\ClinicVets.exe" (
    echo.
    echo RunApp\ClinicVets.exe not found after build.
    echo Try: dotnet build ClinicVets.sln -c Debug
    echo.
    pause
    exit /b 1
)

echo.
echo Starting RunApp\ClinicVets.exe ^(same as double-clicking the EXE in RunApp^)...
echo.

start /wait "" "%~dp0RunApp\ClinicVets.exe"
exit /b 0
