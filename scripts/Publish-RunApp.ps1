# Builds a self-contained Windows x64 app into RunApp/ClinicVets.exe and embeds the application icon.
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $Root "Source\ClinicVetsAvalonia.csproj"
$OutDir = Join-Path $Root "RunApp"

$CreateIcon = Join-Path $Root "scripts\Create-AppIcon.ps1"
if (Test-Path $CreateIcon) {
    & $CreateIcon
}

Write-Host "Publishing ClinicVets to RunApp..."
if (Test-Path $OutDir) {
    Remove-Item $OutDir -Recurse -Force
}

dotnet publish $Project `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o $OutDir

$legacyExe = Join-Path $OutDir "ClinicVetsAvalonia.exe"
if (Test-Path $legacyExe) {
    Remove-Item $legacyExe -Force
}

$exe = Join-Path $OutDir "ClinicVets.exe"
if (-not (Test-Path $exe)) {
    throw "Publish failed: ClinicVets.exe not found in RunApp"
}

$EmbedIcon = Join-Path $Root "scripts\Embed-ExeIcon.ps1"
& $EmbedIcon -ExePath $exe

# Teacher helper files (survive full RunApp republish)
$dataDir = Join-Path $OutDir "Data"
New-Item -ItemType Directory -Force -Path $dataDir | Out-Null
if (-not (Test-Path (Join-Path $dataDir ".gitkeep"))) {
    New-Item -ItemType File -Path (Join-Path $dataDir ".gitkeep") -Force | Out-Null
}
$howToRun = Join-Path $OutDir "HOW-TO-RUN.txt"
@(
    "ClinicVets - How to run"
    "========================="
    ""
    "For teachers and reviewers:"
    ""
    "1. Go to the main project folder (the folder that contains Run-ClinicVets.bat)"
    "2. Double-click Run-ClinicVets.bat"
    "3. Log in with username: admin12   password: Admin123!"
    ""
    "No Visual Studio or .NET SDK is required."
    ""
    "Data files (created on first run):"
    "  RunApp\Data\ClinicVets.db"
    "  RunApp\Data\ClinicVets.xlsx"
    ""
) | Set-Content -Path $howToRun -Encoding UTF8

Write-Host "Done: $exe"
