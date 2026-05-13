# ClinicVets

Hebrew (RTL) **veterinary clinic desktop** application for Windows.

## Official app (use this)

The **only** supported end-user app is in **`run testapp/`**:

- Avalonia 12 UI (.NET 9)
- SQLite database, models, services, navigation, and all clinic features in one project

| Item | Location |
|------|----------|
| Project file | `run testapp/ClinicVetsAvalonia.csproj` |
| Run from source | `run testapp/Run.bat` or `dotnet run --project "run testapp/ClinicVetsAvalonia.csproj"` |
| Publish EXE | `run testapp/Publish-Avalonia-WinX64.ps1` → **`run testapp/Publish/ClinicVets.exe`** (and **`RunApp/ClinicVets.exe`** after a full solution build, which syncs from this project) |

See **`run testapp/README.md`** for the folder layout (Views, Repositories, `AppUi/Styles`, etc.).

## Legacy (not the hand-in EXE)

- **WinForms** stack: `src/Frontend/ClinicVets.Desktop.csproj` — optional legacy; outputs **`ClinicVetsWinForms.exe`** (not synced to `RunApp/`). Run from source: `src/Frontend/Run-WinForms.bat`.
- **Removed root launchers** that only started another EXE or duplicated entry points: `Run-Windows.bat` (started `RunApp\ClinicVets.exe`), `Run-Avalonia.bat`, and root `Publish-Avalonia-WinX64.ps1` (replaced by scripts inside `run testapp/`).

## Default demo logins (Avalonia)

- **Secretary:** `admin` / `1234`
- **Vet:** `vet` / `1234`

## Password reset email (optional)

Set `CLINIC_GMAIL_ADDRESS` and `CLINIC_GMAIL_APP_PASSWORD` for real Gmail; otherwise the forgot-password flow shows a demo code.

## Full solution

Open **`ClinicVets.sln`**. Set startup project to **ClinicVetsAvalonia** (loads from `run testapp\`). See **`Documentation/REPOSITORY-LAYOUT.md`** for the wider repo map.
