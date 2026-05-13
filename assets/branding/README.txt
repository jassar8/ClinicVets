ClinicVets branding (source of truth)

- ClinicVetsLogo.png — official logo / icon artwork (PNG with alpha).
- Regenerate the Windows multi-size ICO after changing the logo (from repo root):

  dotnet run --project tooling/BuildIcon/BuildIcon.csproj -c Release -- assets/branding/ClinicVetsLogo.png assets/app/ClinicVets.ico

- The desktop project embeds the ICO and this PNG at build time (see `src/Frontend/ClinicVets.Desktop.csproj`).

Shortcuts and Explorer

- Shortcuts must target ClinicVets.exe (not ClinicVets.dll). After publishing, recreate the
  desktop shortcut so IconLocation points at the new EXE:

  powershell -ExecutionPolicy Bypass -File docs/Scripts/Update-ClinicVets-DesktopShortcut.ps1 -ExePath PublishedApp/ClinicVets.exe

- If Windows still shows an old generic icon, refresh the shell icon cache, then sign out or restart Explorer:

  ie4uinit.exe -show
