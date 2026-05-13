param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

Write-Host "=== ClinicVets v4: publish WinForms (v2-style) + Avalonia (v3) ===" -ForegroundColor Cyan
Write-Host ""

& (Join-Path $root "publish-win-x64.ps1") -Configuration $Configuration

Write-Host ""
& (Join-Path $root "Publish-Avalonia-WinX64.ps1") -Configuration $Configuration

Write-Host ""
Write-Host "v4 publish summary:" -ForegroundColor Green
Write-Host "  WinForms (like v2):  RunApp\ClinicVets.exe  (and PublishedApp\)" -ForegroundColor Yellow
Write-Host "  Avalonia (v3 UI):    PublishedApp-Avalonia\ClinicVetsAvalonia.exe" -ForegroundColor Yellow
