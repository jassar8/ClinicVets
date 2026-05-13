# ClinicVets — official app (`run testapp`)

This folder is the **only supported** Windows desktop build: Avalonia UI (Hebrew RTL), SQLite data store, validation services, and all clinic screens.

## Run (development)

```powershell
cd "run testapp"
dotnet run --project .\ClinicVetsAvalonia.csproj
```

Or double-click **`Run.bat`** in this folder.

## Publish final EXE

```powershell
cd "run testapp"
.\Publish-Avalonia-WinX64.ps1
```

Output: **`Publish\ClinicVetsAvalonia.exe`** (self-contained). Distribute the whole **`Publish`** directory.

## Contents

| Area | Path |
|------|------|
| Styles / colors | `AppUi/Styles/` |
| SQLite schema + paths | `ClinicDatabase/` |
| Data access | `Repositories/AppData.cs` |
| Models | `Models/` |
| Services | `Services/` |
| Views | `Views/` |
| Session | `ViewModels/AppSession.cs` |
