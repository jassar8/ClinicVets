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
| [`Source/Assets/`](Source/Assets/) | `app.manifest` and other assets |
| [`Tests/`](Tests/) | Automated tests |
| [`Tests/ClinicVets.Tests/`](Tests/ClinicVets.Tests/) | xUnit project |
| [`Tests/ClinicVets.Tests/FunctionalTests/`](Tests/ClinicVets.Tests/FunctionalTests/) | Unit / functional tests |
| [`Tests/ClinicVets.Tests/IntegrationTests/`](Tests/ClinicVets.Tests/IntegrationTests/) | Integration-style tests |
| [`Tests/ClinicVets.Tests/GuiTests/`](Tests/ClinicVets.Tests/GuiTests/) | Placeholder for future GUI tests |
| [`Documentation/`](Documentation/) | Test cases, roadmap, reports (course docs) |
| [`RunApp/`](RunApp/) | **Runnable build** for reviewers (self-contained Windows x64) |
| [`scripts/`](scripts/) | Build helpers (`Publish-RunApp.ps1`) |

## Run the app (teachers / reviewers)

1. Open the `RunApp` folder.
2. Double-click **`ClinicVets.exe`** (Windows x64, self-contained; no .NET SDK required).

Or use **`Run-Windows.bat`** at the repository root.

**Demo login:** user `admin` (Secretary role — clients, animals). Database is stored under `%AppData%\ClinicVetsAvalonia\clinic.db` (not beside the exe).

## Develop

**Requirements:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

```powershell
dotnet build ClinicVets.sln
dotnet test Tests/ClinicVets.Tests/ClinicVets.Tests.csproj
dotnet run --project Source/ClinicVetsAvalonia.csproj
```

**Refresh `RunApp/` after code changes:**

```powershell
powershell -File scripts/Publish-RunApp.ps1
```

## Tests

```powershell
dotnet test Tests/ClinicVets.Tests/ClinicVets.Tests.csproj
```

## Documentation

Add LaTeX, PDF, or Word files under:

- `Documentation/TestCases/`
- `Documentation/Roadmap/`
- `Documentation/Reports/`

## Solution

Open [`ClinicVets.sln`](ClinicVets.sln) in Visual Studio or Rider.
