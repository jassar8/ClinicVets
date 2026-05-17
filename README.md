# ClinicVets (fix branch)

Veterinary clinic management desktop app — Hebrew RTL UI, SQLite storage, employee login, clients, animals, visits, and medicine inventory.

## Folder structure

```
RunApp/                 ← sole official application (build & EXE)
  Views/Auth/           Login, register, forgot password
  Views/Dashboard/      Main menu & role-based navigation
  Views/Medicine/       Medicine inventory CRUD + search
  Views/Employees/      Employee registration
  Views/Clients/        Clients
  Views/Animals/        Animals
  Views/Visits/         Visits
  Views/Shared/         Shared UI helpers
  ViewModels/           Session state
  Models/               Domain models
  Services/             Validation, password reset, filters
  Repositories/         AppData + SQLite sync
  Database/             Schema & paths
  Styles/               Theme colors & control styles
  Assets/Images/        Image placeholders
  Assets/Icons/         Icon placeholders
  Helpers/              UI messages
  Converters/           (reserved)
Tests/ClinicVets.Avalonia.Unit/   Automated unit tests
```

## Main features

- Employee login (default users: `admin` / `vet`, password `1234`)
- Employee registration with validation
- Password reset (demo mode without Gmail env vars)
- Role-based menu (Secretary vs Vet)
- Clients, animals, visits
- Medicine add / update / delete / search / filters
- SQLite database under `%AppData%\ClinicVets\clinic.db`

## How to run (development)

```powershell
cd RunApp
dotnet run --project ClinicVets.csproj
```

Or double-click **`RunApp\Run.bat`**.

Requires **.NET 9 SDK** (Windows).

## How to build the EXE

```powershell
cd RunApp
.\Publish-Avalonia-WinX64.ps1
```

Or from repo root:

```powershell
dotnet publish RunApp\ClinicVets.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o RunApp\Publish
```

## Final EXE location

**`RunApp\Publish\ClinicVets.exe`** — distribute the **entire `Publish` folder** together.

Debug build (requires .NET runtime on machine):

**`RunApp\bin\Release\net9.0-windows\ClinicVets.exe`**

## Tests

```powershell
dotnet test ClinicVets.sln -c Release
```

## Rebuild notes

This branch was rebuilt from the best Avalonia logic on **v10/v4** (organized views, repositories, validation). **updateapp** was not merged — only general styling ideas (turquoise theme, rounded controls).

Backup before rebuild: tag **`backup/fix-pre-rebuild-2026-05-14`**.
