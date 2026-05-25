# Removes old ClinicVets shortcuts and creates a new desktop shortcut to RunApp/ClinicVets.exe.
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$ExePath = (Resolve-Path (Join-Path $Root "RunApp\ClinicVets.exe")).Path

if (-not (Test-Path $ExePath)) {
    throw "Build RunApp first: powershell -File scripts\Publish-RunApp.ps1"
}

$searchPaths = @(
    [Environment]::GetFolderPath("Desktop"),
    (Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs")
)

foreach ($folder in $searchPaths) {
    if (-not (Test-Path $folder)) { continue }
    Get-ChildItem -Path $folder -Filter "ClinicVets*.lnk" -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Host "Removing old shortcut: $($_.FullName)"
        Remove-Item $_.FullName -Force
    }
}

$Desktop = [Environment]::GetFolderPath("Desktop")
$ShortcutPath = Join-Path $Desktop "ClinicVets.lnk"

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($ShortcutPath)
$shortcut.TargetPath = $ExePath
$shortcut.WorkingDirectory = Split-Path $ExePath -Parent
$shortcut.Description = "ClinicVets - Veterinary Clinic Management"
$shortcut.IconLocation = "$ExePath,0"
$shortcut.Save()

Write-Host "Shortcut created: $ShortcutPath"
Write-Host "IconLocation: $($shortcut.IconLocation)"
