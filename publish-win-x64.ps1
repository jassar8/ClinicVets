param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$publishPath = ".\PublishedApp"

Write-Host "Publishing ClinicVets (WinForms desktop) for Windows x64..." -ForegroundColor Cyan

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
    throw "Publish failed. Close any running ClinicVets.exe, then run this script again."
}

# MSBuild AfterTargets=Publish also syncs RunApp; call again so RunApp matches this script's output folder if paths differ.
$sync = Join-Path $PSScriptRoot "docs\Scripts\Sync-RunApp.ps1"
$runApp = Join-Path $PSScriptRoot "RunApp"
& powershell -NoProfile -ExecutionPolicy Bypass -File $sync -Source (Resolve-Path $publishPath).Path -RunAppRoot (Resolve-Path $runApp).Path

Write-Host ""
Write-Host "Publish complete." -ForegroundColor Green
Write-Host "Portable app (keep the whole folder together):" -ForegroundColor Yellow
Write-Host (Join-Path $PSScriptRoot "RunApp\ClinicVets.exe")
Write-Host ""
Write-Host "The same files were synced to RunApp\ (see RunApp\README.md)." -ForegroundColor Green
Write-Host "Optional desktop shortcut:" -ForegroundColor Cyan
Write-Host "  powershell -ExecutionPolicy Bypass -File .\docs\Scripts\Create-ClinicVets-DesktopShortcut.ps1"
