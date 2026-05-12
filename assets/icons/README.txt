ClinicVets Windows shell icon

The multi-resolution `.ico` used for the EXE, Explorer, shortcuts, taskbar, and title bar
is maintained next to the desktop project at:

  src/ClinicVets.Desktop/Assets/app.ico

Regenerate it from the logo PNG (repo root):

  dotnet run --project tools/BuildIcon/BuildIcon.csproj -c Release -- assets/branding/ClinicVetsLogo.png src/ClinicVets.Desktop/Assets/app.ico
