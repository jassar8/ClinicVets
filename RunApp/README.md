# ClinicVets — RunApp (ready-to-run build)

This folder is **filled automatically** when you build or publish the desktop project. It is **not** where source code lives.

## Quick start (development / UI demo)

1. From the repository root, build once:
   ```powershell
   dotnet build .\ClinicVets.sln -c Release
   ```
2. Double-click **`ClinicVets.exe`** in this folder.
3. **Demo Mode (fast UI testing)**  
   On the login screen, click **Enter Demo Mode** (if shown). You skip sign-in and get an in-memory **Demo Admin** workspace with sample customers, animals, visits, and pending employees.  
   **Sign out** returns you to normal login. Demo data does **not** touch your real JSON files under `%LocalAppData%\ClinicVets`.

> **Requires .NET 9** Windows desktop runtime for a normal `dotnet build` output. If the EXE does not start, install [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) (x64), or use the self-contained publish below.

## Disable Demo Mode (hand-in / teacher build)

1. Open **`src/Frontend/DesktopBuildOptions.cs`** in the repo.
2. Change:
   ```csharp
   public const bool EnableDemoMode = false;
   ```
3. Rebuild (`dotnet build` or publish). The Demo Mode button disappears and demo navigation is disabled.

## Publish a portable folder for the teacher (no runtime install)

From the repository root:

```powershell
.\publish-win-x64.ps1
```

That produces a **self-contained** `ClinicVets.exe` (and supporting files) and syncs them here. Zip the **entire `RunApp` folder** so paths stay intact.

## What not to do

- Do not add **source code** only under `RunApp/` — it will be deleted on the next sync.
- Keep **everything** in this folder together when you copy or zip it; the EXE loads DLLs next to it.

For the full map of folders (Frontend, Backend, Models, Services, Data, Tests, Assets), see **`Documentation/REPOSITORY-LAYOUT.md`**.
