@echo off
REM ClinicVets - double-click to start (no Visual Studio required)
cd /d "%~dp0"

set "EXE=%~dp0RunApp\ClinicVets.exe"

if exist "%EXE%" (
    echo Starting ClinicVets...
    start "" "%EXE%"
    exit /b 0
)

echo.
echo  ClinicVets.exe was not found in RunApp\
echo.
echo  For teachers: download the full project from GitHub.
echo  The RunApp folder must contain ClinicVets.exe.
echo.
echo  For developers only - build the EXE with:
echo    powershell -File scripts\Publish-RunApp.ps1
echo.
pause
exit /b 1
