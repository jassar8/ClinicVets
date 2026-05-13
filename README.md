# ClinicVets

Hebrew (RTL) **veterinary clinic desktop** application for Windows. This repository contains two UI stacks:

| App | Technology | Entry |
|-----|------------|--------|
| **ClinicVetsAvalonia** (recommended for review) | Avalonia 12, .NET 9 | `ClinicVetsAvalonia.csproj` at repo root |
| ClinicVets.Desktop | WinForms (larger solution) | `src/Frontend/ClinicVets.Desktop.csproj` in `ClinicVets.sln` |

The Avalonia app is self-contained at the repository root with its own SQLite database, login, employee registration, password reset flow, and modules for clients, animals, visits, and medicine inventory.

## Avalonia app — folder layout

| Folder | Role |
|--------|------|
| `AppUi/Styles/` | Shared colors (`ThemeColors.axaml`) and control styles (`AppTheme.axaml`) |
| `ClinicDatabase/` | SQLite path helpers and schema bootstrap (named to avoid the `database/**` compile exclude used for JSON docs) |
| `Repositories/` | `AppData` — in-memory lists synchronized with SQLite (employees, clients, animals, medications, visits) |
| `Models/` | Entity types used by the UI and persistence |
| `Services/` | `ValidationService`, `PasswordResetService` (optional Gmail SMTP) |
| `Helpers/` | `UIHelper` for dialogs |
| `ViewModels/` | `AppSession` — current signed-in employee (expand here for MVVM) |
| `Views/Auth/` | Login, forgot password |
| `Views/Employees/` | New employee self-registration |
| `Views/Dashboard/` | Main menu and navigation by role |
| `Views/Clients/`, `Views/Animals/`, `Views/Visits/`, `Views/Medicine/` | Domain screens |
| `Views/Shared/` | Shared layout constants (`UiDimensions`) |

Legacy WinForms + layered backend live under `src/`, `Tests/`, `assets/`, and `database/` (JSON runtime notes only — not the Avalonia SQLite code).

## How to run (Avalonia)

```powershell
dotnet run --project .\ClinicVetsAvalonia.csproj
```

Or double-click **`Run-Avalonia.bat`**.

## How to build a portable EXE

```powershell
.\Publish-Avalonia-WinX64.ps1
```

Output folder: **`PublishedApp-Avalonia/`** — keep all files next to `ClinicVetsAvalonia.exe`, or use single-file publish as configured in the script.

Self-contained build does not require .NET on the target PC.

## Default demo logins

After a fresh database, two employees are created automatically:

- **Secretary:** `admin` / `1234`
- **Vet:** `vet` / `1234`

## Password reset email (optional)

To send real Gmail instead of demo on-screen code, set environment variables `CLINIC_GMAIL_ADDRESS` and `CLINIC_GMAIL_APP_PASSWORD` (app password), then use “שכחתי סיסמה”.

## Full solution (WinForms + tests)

Open **`ClinicVets.sln`**. See **`Documentation/REPOSITORY-LAYOUT.md`** for the WinForms and backend map.
