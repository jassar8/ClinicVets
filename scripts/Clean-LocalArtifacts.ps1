# Removes local build artifacts and obsolete folders. Does not touch RunApp, source, or Documentation.
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot

$pathsToRemove = @(
    (Join-Path $Root "bin"),
    (Join-Path $Root "obj"),
    (Join-Path $Root "Source\bin"),
    (Join-Path $Root "Source\obj"),
    (Join-Path $Root "Tests\ClinicVetsAvalonia.Tests"),
    (Join-Path $Root "Helpers"),
    (Join-Path $Root ".vs"),
    (Join-Path $Root ".vscode"),
    (Join-Path $Root ".DS_Store")
)

$testProject = Join-Path $Root "Tests\ClinicVets.Tests"
if (Test-Path $testProject) {
    Get-ChildItem -Path $testProject -Recurse -Directory -Filter bin -ErrorAction SilentlyContinue | ForEach-Object { $pathsToRemove += $_.FullName }
    Get-ChildItem -Path $testProject -Recurse -Directory -Filter obj -ErrorAction SilentlyContinue | ForEach-Object { $pathsToRemove += $_.FullName }
}

foreach ($path in $pathsToRemove | Select-Object -Unique) {
    if (Test-Path $path) {
        Write-Host "Removing: $path"
        Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "Local artifact cleanup complete."
