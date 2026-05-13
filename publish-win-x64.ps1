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

Write-Host ""
Write-Host "Publish complete." -ForegroundColor Green
Write-Host "Run this file (keep the whole PublishedApp folder together):" -ForegroundColor Yellow
Write-Host "$publishPath\ClinicVets.exe"
