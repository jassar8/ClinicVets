param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$publishPath = Join-Path $root "Publish"
$project = Join-Path $root "ClinicVetsAvalonia.csproj"

Write-Host "Publishing official ClinicVets v3 (Avalonia) for Windows x64..." -ForegroundColor Cyan
Write-Host "Output: ClinicVets.exe (synced to repo RunApp\ on build/publish)." -ForegroundColor Cyan

Get-Process | Where-Object { $_.ProcessName -like '*ClinicVets*' } | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500

dotnet publish $project `
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

$exe = Join-Path $publishPath "ClinicVets.exe"
Write-Host ""
Write-Host "Publish complete." -ForegroundColor Green
Write-Host "Official portable EXE (keep entire Publish folder together):" -ForegroundColor Yellow
Write-Host $exe
Write-Host ""
Write-Host "After 'dotnet build' on the solution, RunApp\ClinicVets.exe is also updated from this project." -ForegroundColor Green
