# Embeds ClinicVets.ico into a Windows PE executable using rcedit (guaranteed Shell icon).
param(
    [string]$ExePath = "",
    [string]$IcoPath = ""
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($ExePath)) {
    $ExePath = Join-Path $Root "RunApp\ClinicVets.exe"
}
if ([string]::IsNullOrWhiteSpace($IcoPath)) {
    $IcoPath = Join-Path $Root "Source\Assets\ClinicVets.ico"
}

$Rcedit = Join-Path $Root "scripts\tools\rcedit-x64.exe"
if (-not (Test-Path $Rcedit)) {
    throw "rcedit not found. Expected: $Rcedit"
}
if (-not (Test-Path $ExePath)) {
    throw "EXE not found: $ExePath"
}
if (-not (Test-Path $IcoPath)) {
    throw "ICO not found: $IcoPath. Run scripts\Create-AppIcon.ps1 first."
}

$beforeSize = (Get-Item $ExePath).Length
& $Rcedit $ExePath --set-icon $IcoPath
if ($LASTEXITCODE -ne 0) {
    throw "rcedit failed with exit code $LASTEXITCODE"
}

Add-Type -AssemblyName System.Drawing
$icon = [System.Drawing.Icon]::ExtractAssociatedIcon($ExePath)
if ($null -eq $icon) {
    throw "Icon verification failed: ExtractAssociatedIcon returned null for $ExePath"
}

$afterSize = (Get-Item $ExePath).Length
Write-Host "Icon embedded: $ExePath"
Write-Host "Verified: $($icon.Width)x$($icon.Height) icon extractable (EXE size $beforeSize -> $afterSize bytes)"
$icon.Dispose()
