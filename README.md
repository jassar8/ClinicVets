# ClinicVets

Veterinary clinic management — **Windows desktop** app (.NET 9, WinForms).

## Where everything lives

| Area | Folder |
|------|--------|
| **Frontend** (WinForms UI) | `src/Frontend/` — `Forms/`, `UI/`, `UserControls/` |
| **Backend** (logic + data) | `src/Backend/` — `ClinicVets.Core`, `ClinicVets.Application`, `ClinicVets.Infrastructure` |
| **Tests** | `Tests/ClinicVets.Tests/` — `Functional/`, `Integration/`, `GUI/` |
| **Documentation** | `docs/` — charter, scripts |
| **Shared assets** | `assets/` — branding (`branding/`), app icon (`app/`) |
| **Persistence notes** | `database/README.md` — where JSON data is stored at runtime |
| **Tooling** | `tooling/BuildIcon` — optional dev utility to rebuild the `.ico` |
| **Quick run (after build/publish)** | `RunApp/ClinicVets.exe` — synced automatically from the latest build or publish output |
| **Published EXE** | `PublishedApp/ClinicVets.exe` (created by `publish-win-x64.ps1`; also copied to `RunApp/`) |

## Run from source

Open `ClinicVets.sln` in Visual Studio and set **ClinicVets.Desktop** as startup, or from the repo root:

```powershell
dotnet run --project .\src\Frontend\ClinicVets.Desktop.csproj
```

## Build the standalone Windows EXE

```powershell
.\publish-win-x64.ps1
```

Then run **`PublishedApp\ClinicVets.exe`** (keep the whole `PublishedApp` folder together).

## Desktop shortcut

```powershell
powershell -ExecutionPolicy Bypass -File .\docs\Scripts\Update-ClinicVets-DesktopShortcut.ps1 -ExePath .\PublishedApp\ClinicVets.exe
```

## More detail

See [docs/Project-Documentation.md](docs/Project-Documentation.md) for the full project charter, architecture notes, and data paths.
