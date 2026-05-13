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
Write-Host "  WinForms legacy:     PublishedApp\ClinicVetsWinForms.exe" -ForegroundColor Yellow
Write-Host "  Official v3 (main):  run testapp\Publish\ClinicVets.exe  (and RunApp\ after Avalonia sync)" -ForegroundColor Yellow
