ClinicVets branding (source of truth)

- ClinicVetsLogo.png — official logo / icon artwork (PNG with alpha).
- Regenerate the Windows multi-size ICO after changing the logo (from repo root):

  dotnet run --project Tooling/BuildIcon/BuildIcon.csproj -c Release -- assets/branding/ClinicVetsLogo.png Frontend/Assets/ClinicVets.ico

- The desktop project also copies this PNG into Frontend/Assets for embedding at build time.

Shortcuts and Explorer

- Shortcuts must target ClinicVets.exe (not ClinicVets.dll). After publishing, recreate the
  desktop shortcut so IconLocation points at the new EXE:

  powershell -ExecutionPolicy Bypass -File Documentation/Scripts/Update-ClinicVets-DesktopShortcut.ps1 -ExePath PublishedApp/ClinicVets.exe

- If Windows still shows an old generic icon, refresh the shell icon cache, then sign out or restart Explorer:

  ie4uinit.exe -show
