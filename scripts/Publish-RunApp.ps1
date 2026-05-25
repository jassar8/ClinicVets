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

Write-Host "Done: $exe"
