# ClinicVets

Veterinary clinic management — **Windows desktop** app (.NET 9, WinForms).

## Where everything lives

| Area | Folder |
|------|--------|
| **Frontend** (WinForms UI) | `Frontend/` — `Forms/`, `UI/`, `UserControls/`, `Assets/` |
| **Backend** (logic + data) | `Backend/` — `ClinicVets.Core`, `ClinicVets.Application`, `ClinicVets.Infrastructure` |
| **Tests** | `Tests/ClinicVets.Tests/` — `Functional/`, `Integration/`, `GUI/` |
| **Documentation** | `Documentation/` — charter, scripts |
| **Published EXE** | `PublishedApp/ClinicVets.exe` (created by the publish script) |

## Run from source

Open `clinicVets.sln` in Visual Studio and set **ClinicVets.Desktop** as startup, or from the repo root:

```powershell
dotnet run --project .\Frontend\ClinicVets.Desktop.csproj
```

## Build the standalone Windows EXE

```powershell
.\publish-win-x64.ps1
```

Then run **`PublishedApp\ClinicVets.exe`** (keep the whole `PublishedApp` folder together).

## Desktop shortcut

```powershell
powershell -ExecutionPolicy Bypass -File .\Documentation\Scripts\Update-ClinicVets-DesktopShortcut.ps1 -ExePath .\PublishedApp\ClinicVets.exe
```

## More detail

See [Documentation/Project-Documentation.md](Documentation/Project-Documentation.md) for the full project charter, architecture notes, and data paths.
