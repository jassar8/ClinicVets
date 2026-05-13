param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

Write-Host "=== ClinicVets v4: publish WinForms (v2-style) + official Avalonia app ===" -ForegroundColor Cyan
Write-Host ""

& (Join-Path $root "publish-win-x64.ps1") -Configuration $Configuration

Write-Host ""
& (Join-Path $root "run testapp\Publish-Avalonia-WinX64.ps1") -Configuration $Configuration

Write-Host ""
Write-Host "v4 publish summary:" -ForegroundColor Green
Write-Host "  WinForms (legacy):   RunApp\ClinicVets.exe  (and PublishedApp\)" -ForegroundColor Yellow
Write-Host "  Official Avalonia:   run testapp\Publish\ClinicVetsAvalonia.exe" -ForegroundColor Yellow
