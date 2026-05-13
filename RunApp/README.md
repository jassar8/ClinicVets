# ClinicVets — RunApp (official v3 build mirror)

This folder is **filled automatically** when you **build** or **publish** the **official Avalonia app** in **`run testapp/`** (`ClinicVetsAvalonia.csproj` → output **`ClinicVets.exe`**).

It is **not** source code. Anything here except `README*.md` is replaced on the next sync.

## What EXE this is

- **`ClinicVets.exe` here = your Hebrew v3 Avalonia app** (login, SQLite, clients, animals, visits, medicines).
- Legacy WinForms builds **`ClinicVetsWinForms.exe`** and does **not** sync to this folder anymore.

## Quick start

1. From repo root:
   ```powershell
   dotnet build .\ClinicVets.sln -c Release
   ```
2. Double-click **`ClinicVets.exe`** in this folder.

For a **self-contained** folder (teacher PC without .NET):

```powershell
.\run testapp\Publish-Avalonia-WinX64.ps1
```

Then use **`run testapp\Publish\ClinicVets.exe`** (and the rest of that folder), or build the solution so **`RunApp`** is refreshed again.

## Data folder

SQLite and app data: **`%AppData%\ClinicVets\`** (v3 official path).  
If you used an older build that stored data under `ClinicVetsAvalonia`, copy `clinic.db` into `%AppData%\ClinicVets\` if you need to migrate.

## What not to do

- Do not put source-only files here — they are deleted on sync.
- Zip the **entire** `RunApp` folder when sharing so DLLs stay next to the EXE.
