# ClinicVets

Veterinary clinic management — **Windows desktop** app (.NET 9, WinForms). One main window; modern UI; optional **Demo Mode** for fast UI testing.

## Fastest way to run (after build)

Double-click **`RunApp/ClinicVets.exe`** (entire `RunApp` folder must stay together). This copy is refreshed on every **`dotnet build`** or **`.\publish-win-x64.ps1`**.

- **After `dotnet build`**: you need the [.NET 9 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/9.0) installed.
- **After `.\publish-win-x64.ps1`**: self-contained Windows x64 app — good for zipping for a teacher PC without .NET installed.

See **`RunApp/README.md`** for Demo Mode, disabling demo, and publish tips.

## Where everything lives

Full map: **[Documentation/REPOSITORY-LAYOUT.md](Documentation/REPOSITORY-LAYOUT.md)**

| Area | Folder |
|------|--------|
| **Frontend** (WinForms UI, Demo Mode) | `src/Frontend/` |
| **Backend** | `src/Backend/` — Core, Application, Infrastructure |
| **Models** | `src/Backend/ClinicVets.Core/Models/`, `Entities/` |
| **Services** | `src/Backend/ClinicVets.Application/` |
| **Data** (JSON + in-memory demo stores) | `src/Backend/ClinicVets.Infrastructure/Data/` |
| **Tests** | `Tests/ClinicVets.Tests/` |
| **Assets** | `assets/` — icon, logo (embedded in desktop project) |
| **Documentation** | `Documentation/`, `docs/` |
| **Runtime data (JSON)** | `%LocalAppData%\ClinicVets\` — see `database/README.md` |
| **Runnable output** | `RunApp/` — **not** source; synced from build |

## Disable Demo Mode (final hand-in)

In **`src/Frontend/DesktopBuildOptions.cs`**, set `EnableDemoMode = false`, then rebuild.

## Run from source / IDE

Open `ClinicVets.sln`, set **ClinicVets.Desktop** as startup, or:

```powershell
dotnet run --project .\src\Frontend\ClinicVets.Desktop.csproj
```

## Portable publish (teacher machine)

```powershell
.\publish-win-x64.ps1
```

Then zip the whole **`RunApp`** folder (or use `PublishedApp` — the script mirrors to `RunApp`).

## More detail

- [docs/Project-Documentation.md](docs/Project-Documentation.md) — charter, architecture  
- [Documentation/REPOSITORY-LAYOUT.md](Documentation/REPOSITORY-LAYOUT.md) — folder map and RunApp behavior
