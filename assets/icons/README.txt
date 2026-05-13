ClinicVets Windows shell icon

The multi-resolution `.ico` used for the EXE, Explorer, shortcuts, taskbar, and title bar
is maintained at:

  assets/app/ClinicVets.ico

Regenerate it from the logo PNG (repo root):

  dotnet run --project tooling/BuildIcon/BuildIcon.csproj -c Release -- assets/branding/ClinicVetsLogo.png assets/app/ClinicVets.ico
