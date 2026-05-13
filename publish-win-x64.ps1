param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$publishPath = ".\PublishedApp"

Write-Host "Publishing legacy WinForms (ClinicVetsWinForms.exe) for Windows x64..." -ForegroundColor Cyan
Write-Host "NOTE: Official RunApp\ClinicVets.exe comes from run testapp\Publish-Avalonia-WinX64.ps1 (v3 Avalonia)." -ForegroundColor Yellow
Write-Host ""

Get-Process | Where-Object { $_.ProcessName -like '*ClinicVets*' } | Stop-Process -Force -ErrorAction SilentlyContinue

dotnet publish .\src\Frontend\ClinicVets.Desktop.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:PublishTrimmed=false `
    -o $publishPath

if ($LASTEXITCODE -ne 0) {
    throw "Publish failed. Close any running ClinicVetsWinForms.exe or ClinicVets.exe, then try again."
}

Write-Host ""
Write-Host "Publish complete (WinForms legacy)." -ForegroundColor Green
Write-Host "Portable WinForms EXE (keep the whole PublishedApp folder together):" -ForegroundColor Yellow
Write-Host (Join-Path $PSScriptRoot "PublishedApp\ClinicVetsWinForms.exe")
Write-Host ""
Write-Host "For the official Hebrew v3 app and RunApp mirror, run:" -ForegroundColor Cyan
Write-Host "  .\run testapp\Publish-Avalonia-WinX64.ps1"
