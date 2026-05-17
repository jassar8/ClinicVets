param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$publishPath = Join-Path $root "Publish"
$project = Join-Path $root "ClinicVets.csproj"

Write-Host "Publishing ClinicVets (fix branch) for Windows x64..." -ForegroundColor Cyan
Write-Host "Output folder: $publishPath" -ForegroundColor Cyan

Get-Process | Where-Object { $_.ProcessName -like '*ClinicVets*' } | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500

if (Test-Path $publishPath) {
    Remove-Item -LiteralPath $publishPath -Recurse -Force
}

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
Write-Host "Portable EXE (keep the entire Publish folder together):" -ForegroundColor Yellow
Write-Host $exe
