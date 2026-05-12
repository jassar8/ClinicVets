ClinicVets branding (source of truth)

- ClinicVetsLogo.png — official logo / icon artwork (PNG with alpha).
- Regenerate the Windows multi-size ICO after changing the logo (from repo root):

  dotnet run --project tools/BuildIcon/BuildIcon.csproj -c Release -- assets/branding/ClinicVetsLogo.png src/ClinicVets.Desktop/Assets/app.ico

Shortcuts and Explorer

- Shortcuts must target ClinicVets.exe (not ClinicVets.dll). After publishing, recreate the
  desktop shortcut so IconLocation points at the new EXE:

  powershell -ExecutionPolicy Bypass -File scripts/Update-ClinicVets-DesktopShortcut.ps1 -ExePath publish/win-x64/ClinicVets.exe

- If Windows still shows an old generic icon, refresh the shell icon cache, then sign out or restart Explorer:

  ie4uinit.exe -show
