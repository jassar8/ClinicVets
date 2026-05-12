ClinicVets branding (source of truth)

- ClinicVetsLogo.png — official logo / icon artwork (PNG with alpha).
- Regenerate the Windows multi-size ICO after changing the logo (from repo root):

  dotnet run --project tools/BuildIcon/BuildIcon.csproj -c Release -- assets/branding/ClinicVetsLogo.png assets/icons/app.ico
