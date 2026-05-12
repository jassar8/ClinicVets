ClinicVets Windows shell icon

The multi-resolution `.ico` used for the EXE, Explorer, shortcuts, taskbar, and title bar
is maintained with the desktop project at:

  Frontend/Assets/ClinicVets.ico

Regenerate it from the logo PNG (repo root):

  dotnet run --project Tooling/BuildIcon/BuildIcon.csproj -c Release -- assets/branding/ClinicVetsLogo.png Frontend/Assets/ClinicVets.ico
