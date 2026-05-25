# Refreshes Windows icon cache so Explorer shows updated EXE icons.
$ErrorActionPreference = "Stop"

Write-Host "Refreshing Windows icon cache..."
$ie4u = Join-Path $env:SystemRoot "System32\ie4uinit.exe"
if (Test-Path $ie4u) {
    & $ie4u -show
    Write-Host "Ran ie4uinit.exe -show"
}
else {
    Write-Host "ie4uinit.exe not found; skipping."
}

Write-Host "If the icon still looks wrong, sign out and back in, or restart Explorer from Task Manager."
