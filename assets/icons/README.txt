ClinicVets application icon (Windows)

- app.ico — multi-resolution ICO (16–256 px) used for the EXE, Explorer, taskbar, and window title bars.
- generate_app_icon.py — regenerates app.ico from the same palette as UiTheme.HeaderBlue (Pillow required: pip install Pillow).

The desktop project references app.ico via ApplicationIcon and embeds a copy for WinForms window chrome.
