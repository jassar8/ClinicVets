# Generates Source/Assets/ClinicVets.ico from ClinicVetsLogo.png (multi-size, transparent background).
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$PngPath = Join-Path $Root "Source\Assets\ClinicVetsLogo.png"
$IcoPath = Join-Path $Root "Source\Assets\ClinicVets.ico"

if (-not (Test-Path $PngPath)) {
    throw "Logo not found: $PngPath"
}

function Test-IconFile {
    param([string]$Path)
    if (-not (Test-Path $Path)) { return $false }
    $length = (Get-Item $Path).Length
    if ($length -lt 1024 -or $length -gt 2MB) { return $false }
    try {
        Add-Type -AssemblyName System.Drawing
        $icon = New-Object System.Drawing.Icon $Path
        $icon.Dispose()
        return $true
    }
    catch {
        return $false
    }
}

function New-IconWithMagick {
    param([string]$Png, [string]$Ico)
    $magick = Get-Command magick -ErrorAction SilentlyContinue
    if (-not $magick) { return $false }

    & $magick.Source convert $Png `
        -background none `
        -alpha on `
        -define "icon:auto-resize=256,48,32,16" `
        $Ico

    if ($LASTEXITCODE -ne 0) { return $false }
    return (Test-IconFile $Ico)
}

function New-TransparentBitmap {
    param([System.Drawing.Bitmap]$Source, [int]$WhiteThreshold = 245)

    $result = New-Object System.Drawing.Bitmap $Source.Width, $Source.Height, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    for ($y = 0; $y -lt $Source.Height; $y++) {
        for ($x = 0; $x -lt $Source.Width; $x++) {
            $pixel = $Source.GetPixel($x, $y)
            if ($pixel.A -gt 0 -and $pixel.R -ge $WhiteThreshold -and $pixel.G -ge $WhiteThreshold -and $pixel.B -ge $WhiteThreshold) {
                $result.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(0, 255, 255, 255))
            }
            else {
                $result.SetPixel($x, $y, $pixel)
            }
        }
    }
    return $result
}

function New-ResizedBitmap {
    param([System.Drawing.Bitmap]$Source, [int]$Size)

    $bmp = New-Object System.Drawing.Bitmap $Size, $Size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bmp)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.DrawImage($Source, 0, 0, $Size, $Size)
    $graphics.Dispose()
    return $bmp
}

function New-IconWithDrawing {
    param([string]$Png, [string]$Ico)

    Add-Type -AssemblyName System.Drawing
    $source = [System.Drawing.Bitmap]::FromFile($Png)
    $transparent = New-TransparentBitmap $source
    $source.Dispose()

    $sizes = @(16, 32, 48, 256)
    $bitmaps = New-Object System.Collections.Generic.List[System.Drawing.Bitmap]
    foreach ($size in $sizes) {
        $bitmaps.Add((New-ResizedBitmap $transparent $size))
    }
    $transparent.Dispose()

    $largest = $bitmaps[$bitmaps.Count - 1]
    $iconHandle = $largest.GetHicon()
    try {
        $icon = [System.Drawing.Icon]::FromHandle($iconHandle)
        $stream = [System.IO.File]::Open($Ico, [System.IO.FileMode]::Create)
        try {
            $icon.Save($stream)
        }
        finally {
            $stream.Close()
        }
    }
    finally {
        foreach ($bmp in $bitmaps) { $bmp.Dispose() }
    }

    return (Test-IconFile $Ico)
}

if (Test-Path $IcoPath) {
    Remove-Item $IcoPath -Force
}

$created = New-IconWithMagick $PngPath $IcoPath
if (-not $created) {
    Write-Host "ImageMagick not available or failed; using System.Drawing fallback..."
    $created = New-IconWithDrawing $PngPath $IcoPath
}

if (-not $created) {
    throw "Failed to create a valid icon at $IcoPath"
}

$info = Get-Item $IcoPath
Write-Host "Created: $IcoPath ($([math]::Round($info.Length / 1KB, 1)) KB)"
