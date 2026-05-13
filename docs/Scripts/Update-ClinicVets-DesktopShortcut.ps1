param(
    [Parameter(Mandatory = $true, HelpMessage = "Full path to ClinicVets.exe (e.g. PublishedApp\\ClinicVets.exe)")]
    [string] $ExePath
)

$exe = (Resolve-Path -LiteralPath $ExePath).Path
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Executable not found: $exe"
}

$desktop = [Environment]::GetFolderPath('Desktop')
$lnkPath = Join-Path $desktop 'ClinicVets.lnk'

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($lnkPath)
$shortcut.TargetPath = $exe
$shortcut.WorkingDirectory = Split-Path $exe -Parent
# Pin the shortcut to the EXE's first icon group (avoids stale generic shell icons).
$shortcut.IconLocation = "$exe,0"
$shortcut.Description = 'ClinicVets — veterinary clinic desktop app'
$shortcut.Save()

Write-Host "Shortcut updated: $lnkPath"
Write-Host "If File Explorer still shows a generic icon, refresh the icon cache (run in cmd as your user):"
Write-Host "  ie4uinit.exe -show"
