# ClinicVets

Desktop veterinary clinic management application (Avalonia UI, .NET 10, SQLite).

## Project layout

| Folder | Purpose |
|--------|---------|
| [`Source/`](Source/) | Application source code |
| [`Source/Frontend/`](Source/Frontend/) | UI: views, main window, app entry, UI helpers |
| [`Source/Models/`](Source/Models/) | Entity models (client, animal, visit, medication, …) |
| [`Source/Services/`](Source/Services/) | Business rules (validation, password reset) |
| [`Source/Data/`](Source/Data/) | SQLite persistence (`AppData`) |
| [`Source/Backend/`](Source/Backend/) | Architecture notes (desktop app; no HTTP API) |
| [`Source/Assets/`](Source/Assets/) | App manifest, logo PNG, multi-size `ClinicVets.ico` |
| [`Tests/`](Tests/) | Automated tests |
| [`Tests/ClinicVets.Tests/`](Tests/ClinicVets.Tests/) | xUnit project |
| [`Tests/ClinicVets.Tests/FunctionalTests/`](Tests/ClinicVets.Tests/FunctionalTests/) | Unit / functional tests |
| [`Tests/ClinicVets.Tests/IntegrationTests/`](Tests/ClinicVets.Tests/IntegrationTests/) | Integration-style tests |
| [`Tests/ClinicVets.Tests/GuiTests/`](Tests/ClinicVets.Tests/GuiTests/) | Placeholder for future GUI tests |
| [`Documentation/`](Documentation/) | Test cases, roadmap, reports (course docs) |
| [`RunApp/`](RunApp/) | **Runnable build** for reviewers (self-contained Windows x64) |
| [`scripts/`](scripts/) | Build/publish, icon, shortcut, and cleanup scripts (see below) |

## Submission / teacher review

**Quick start (no SDK required):**

1. Open [`RunApp/`](RunApp/) and double-click **`ClinicVets.exe`**, or run [`Run-Windows.bat`](Run-Windows.bat) from the repo root.
2. Log in with a demo account (password for both: `1234`):

| User | Role | Main screens |
|------|------|----------------|
| `admin` | Secretary | Customers (clients), animals |
| `vet` | Vet | Animals, visits, medications |

3. Data is stored in `%AppData%\ClinicVetsAvalonia\clinic.db` (created on first run).

**For developers** (requires [.NET 10 SDK](https://dotnet.microsoft.com/download)):

```powershell
dotnet build ClinicVets.sln
dotnet test Tests/ClinicVets.Tests/ClinicVets.Tests.csproj
dotnet run --project Source/ClinicVetsAvalonia.csproj
```

**After cloning:** `bin/` and `obj/` are not in git. To remove local build output:

```powershell
powershell -File scripts/Clean-LocalArtifacts.ps1
```

**Course documentation:** add PDF/Word/LaTeX files under [`Documentation/TestCases/`](Documentation/TestCases/) and [`Documentation/Reports/`](Documentation/Reports/).

## Run the app (teachers / reviewers)

1. Open the `RunApp` folder.
2. Double-click **`ClinicVets.exe`** (Windows x64, self-contained; no .NET SDK required).

Or use **`Run-Windows.bat`** at the repository root.

**Demo logins:** `admin` / `1234` (Secretary), `vet` / `1234` (Vet). Database is stored under `%AppData%\ClinicVetsAvalonia\clinic.db` (not beside the exe).

## Develop

**Requirements:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

```powershell
dotnet build ClinicVets.sln
dotnet test Tests/ClinicVets.Tests/ClinicVets.Tests.csproj
dotnet run --project Source/ClinicVetsAvalonia.csproj
```

**Refresh `RunApp/` after code changes (includes icon embed):**

```powershell
powershell -File scripts/Publish-RunApp.ps1
powershell -File scripts/Refresh-DesktopShortcut.ps1
powershell -File scripts/Refresh-WindowsIconCache.ps1
```

## Application icon

The clinic logo is stored as [`Source/Assets/ClinicVets.ico`](Source/Assets/ClinicVets.ico) (from [`ClinicVetsLogo.png`](Source/Assets/ClinicVetsLogo.png)). It is embedded into the EXE in two ways:

1. **MSBuild** — `<ApplicationIcon>` in the project file at compile/publish time
2. **rcedit** — [`scripts/Embed-ExeIcon.ps1`](scripts/Embed-ExeIcon.ps1) runs after publish so File Explorer, desktop shortcuts, taskbar, and Alt+Tab always show the correct icon

The same `.ico` is used for the Avalonia window title bar while the app runs.

**Full icon rebuild (recommended after logo changes):**

```powershell
powershell -File scripts/Create-AppIcon.ps1
dotnet build ClinicVets.sln -c Release
powershell -File scripts/Publish-RunApp.ps1
powershell -File scripts/Refresh-DesktopShortcut.ps1
powershell -File scripts/Refresh-WindowsIconCache.ps1
```

**If Windows still shows the old generic icon:** delete any old `ClinicVets*.lnk` on the desktop, run `Refresh-DesktopShortcut.ps1` again, then `Refresh-WindowsIconCache.ps1`. Confirm you are opening `RunApp\ClinicVets.exe`, not an old copy elsewhere.

## Tests

```powershell
dotnet test Tests/ClinicVets.Tests/ClinicVets.Tests.csproj
```

## Documentation

Add LaTeX, PDF, or Word files under:

- `Documentation/TestCases/`
- `Documentation/Reports/`

## Solution

Open [`ClinicVets.sln`](ClinicVets.sln) in Visual Studio or Rider.
