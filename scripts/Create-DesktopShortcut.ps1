# Deprecated wrapper — use Refresh-DesktopShortcut.ps1
$ErrorActionPreference = "Stop"
$refresh = Join-Path $PSScriptRoot "Refresh-DesktopShortcut.ps1"
& $refresh
