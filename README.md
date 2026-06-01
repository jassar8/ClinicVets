# ClinicVets

Desktop veterinary clinic management application (Avalonia UI, .NET 10, SQLite + Excel mirror export).

## HOW TO RUN (for teachers)

1. Open the project folder `ClinicVets`
2. Double-click **`Run-ClinicVets.bat`**
3. Log in with demo user **`admin12`** / password **`Admin123!`**

No Visual Studio or .NET SDK required.

Demo data (customers, animals, medicines, visits) is created automatically on first run. Database and Excel files are saved in **`RunApp/Data/`** next to the executable.

Alternative: open the [`RunApp/`](RunApp/) folder and double-click **`ClinicVets.exe`** directly.

## Project layout

| Folder | Purpose |
|--------|---------|
| [`Source/`](Source/) | Application source code |
| [`Source/Frontend/`](Source/Frontend/) | UI: views, main window, app entry, UI helpers |
| [`Source/Models/`](Source/Models/) | Entity models (client, animal, visit, medication, …) |
| [`Source/Services/`](Source/Services/) | Business rules (validation) |
| [`Source/Data/`](Source/Data/) | SQLite persistence, Excel export (`ClinicVets.xlsx`) |
| [`Source/Backend/`](Source/Backend/) | Architecture notes (desktop app; no HTTP API) |
| [`Source/Assets/`](Source/Assets/) | App manifest, logo PNG, multi-size `ClinicVets.ico` |
| [`Tests/`](Tests/) | Automated tests |
| [`Tests/ClinicVets.Tests/`](Tests/ClinicVets.Tests/) | xUnit project |
| [`Tests/ClinicVets.Tests/FunctionalTests/`](Tests/ClinicVets.Tests/FunctionalTests/) | Unit / functional tests |
| [`Tests/ClinicVets.Tests/IntegrationTests/`](Tests/ClinicVets.Tests/IntegrationTests/) | Integration-style tests |
| [`Tests/ClinicVets.Tests/GuiTests/`](Tests/ClinicVets.Tests/GuiTests/) | Placeholder for future GUI tests |
| [`Documentation/`](Documentation/) | Test cases, roadmap, reports (course docs) |
| [`RunApp/`](RunApp/) | **Runnable EXE** + DLLs (self-contained Windows x64); data in `RunApp/Data/` |
| **`Run-ClinicVets.bat`** | **Double-click launcher** at project root (recommended for teachers) |
| [`scripts/`](scripts/) | Build/publish, icon, shortcut, and cleanup scripts (see below) |

## Submission / teacher review

**Quick start (no SDK required):** see [HOW TO RUN](#how-to-run-for-teachers) above.

**Demo logins** (seed data is created automatically on first run when the database is empty):

| User | Password | Role (app) | Notes |
|------|----------|--------------|-------|
| `admin12` | `Admin123!` | Secretary | Admin-style account (full clinic access as secretary) |
| `secuser` | `Sec123!a` | Secretary | Secretary demo |
| `vetuser` | `Vet123!a` | Vet | Veterinarian demo |
| `sarah1` | `Pass123!` | Secretary | Extra secretary |
| `david2` | `Pass123!` | Vet | Extra veterinarian |
| `roni12` | `Pass123!` | Vet | Extra veterinarian |

Seeded demo also includes **5 customers**, **6 animals**, **6 medicines**, and **6 visits** with treatments.

**Data persistence (course requirement):**

- **SQLite (primary):** `RunApp/Data/ClinicVets.db` when running the published EXE (portable, visible in the project).
- **Excel (mirror):** `RunApp/Data/ClinicVets.xlsx` — updated when you add or change data in the app.
- **Developers** (`dotnet run`): `%LocalAppData%\ClinicVets\` and [`Source/Data/ClinicVets.xlsx`](Source/Data/ClinicVets.xlsx).

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
