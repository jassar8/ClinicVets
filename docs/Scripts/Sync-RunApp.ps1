param(
    [Parameter(Mandatory = $true)]
    [string] $Source,
    [Parameter(Mandatory = $true)]
    [string] $RunAppRoot
)

$ErrorActionPreference = 'Stop'

$src = (Resolve-Path -LiteralPath $Source).Path.TrimEnd('\')
$dest = [System.IO.Path]::GetFullPath($RunAppRoot)

if (-not (Test-Path -LiteralPath $src)) {
    throw "Source path not found: $src"
}

New-Item -ItemType Directory -Path $dest -Force | Out-Null

# Keep any README*.md in RunApp (usage notes survive rebuild/publish sync).
Get-ChildItem -LiteralPath $dest -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notlike 'README*' } |
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Get-ChildItem -LiteralPath $src -Force |
    Copy-Item -Destination $dest -Recurse -Force

Write-Host "Synced RunApp from: $src" -ForegroundColor Green
Write-Host "              to: $dest" -ForegroundColor Green
