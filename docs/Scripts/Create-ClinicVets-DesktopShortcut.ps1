param(
    [string] $ExePath = (Join-Path $PSScriptRoot '..\..\RunApp\ClinicVets.exe')
)

$ErrorActionPreference = 'Stop'
$exe = [System.IO.Path]::GetFullPath($ExePath)

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Executable not found: $exe. Build or publish the app first so RunApp is populated."
}

$desktop = [Environment]::GetFolderPath('Desktop')
$lnkPath = Join-Path $desktop 'ClinicVets.lnk'

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($lnkPath)
$shortcut.TargetPath = $exe
$shortcut.WorkingDirectory = [System.IO.Path]::GetDirectoryName($exe)
$shortcut.IconLocation = "$exe,0"
$shortcut.WindowStyle = 1
$shortcut.Description = 'ClinicVets — veterinary clinic desktop app'
$shortcut.Save()

Write-Host "Desktop shortcut created: $lnkPath" -ForegroundColor Green
Write-Host "Target: $exe" -ForegroundColor Cyan
