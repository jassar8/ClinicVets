param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$publishPath = ".\publish\win-x64"

Write-Host "Publishing ClinicVets for Windows x64..." -ForegroundColor Cyan

Get-Process ClinicVets.Web -ErrorAction SilentlyContinue | Stop-Process -Force

dotnet publish .\src\ClinicVets.Web\ClinicVets.Web.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:PublishTrimmed=false `
    -o $publishPath

if ($LASTEXITCODE -ne 0) {
    throw "Publish failed. Ensure no running ClinicVets.Web.exe instance is locking the publish folder."
}

Write-Host ""
Write-Host "Publish complete." -ForegroundColor Green
Write-Host "Run this file:" -ForegroundColor Yellow
Write-Host "$publishPath\ClinicVets.Web.exe"
