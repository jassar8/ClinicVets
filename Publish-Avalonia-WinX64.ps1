param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$publishPath = Join-Path $root "PublishedApp-Avalonia"
$project = Join-Path $root "ClinicVetsAvalonia.csproj"

Write-Host "Publishing ClinicVetsAvalonia (Avalonia) for Windows x64..." -ForegroundColor Cyan

Get-Process | Where-Object { $_.ProcessName -like '*ClinicVets*' } | Stop-Process -Force -ErrorAction SilentlyContinue
Stop-Process -Name "ClinicVetsAvalonia" -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 800

dotnet publish $project `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:PublishTrimmed=false `
    -o $publishPath

if ($LASTEXITCODE -ne 0) {
    throw "Publish failed. Close any running ClinicVetsAvalonia.exe, then run this script again."
}

$exe = Join-Path $publishPath "ClinicVetsAvalonia.exe"
Write-Host ""
Write-Host "Publish complete." -ForegroundColor Green
Write-Host "Portable folder (keep all files together):" -ForegroundColor Yellow
Write-Host $exe
