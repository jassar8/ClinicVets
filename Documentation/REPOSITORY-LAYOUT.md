# ClinicVets repository layout

This table maps the folders you care about for coursework and demos to where they live in the repo. Paths are relative to the repository root.

| Area | Purpose | Location |
|------|---------|----------|
| **Frontend** | WinForms UI, Demo Mode, shell, pages | `src/Frontend/` |
| **Backend** | All .NET projects that are not the desktop EXE | `src/Backend/` (see sub-areas below) |
| **Models** | Domain entities (customer, animal, employee, enums) | `src/Backend/ClinicVets.Core/Models/`, `.../Entities/` |
| **Services** | Application services, security, validation | `src/Backend/ClinicVets.Application/` |
| **Data** | JSON persistence, in-memory demo stores | `src/Backend/ClinicVets.Infrastructure/Data/` |
| **Tests** | xUnit tests | `Tests/ClinicVets.Tests/` |
| **Assets** | Icons, logos (embedded in the desktop project) | `assets/` |
| **Documentation** | Project docs and layout guides | `Documentation/`, `docs/` |
| **RunApp** | **Runnable output** — copy of the last build/publish (not source) | `RunApp/` |

## Runnable EXE (development and demo)

After **`dotnet build`** (any configuration) or **`.\publish-win-x64.ps1`**, open:

`RunApp/ClinicVets.exe`

- **Framework-dependent build** (`dotnet build`): the machine needs the **.NET 9** desktop runtime installed. All DLLs sit next to the EXE in `RunApp/`.
- **Self-contained publish** (`publish-win-x64.ps1`): `RunApp/` contains a **portable** Windows x64 app (larger, no separate runtime install).

Do not commit large publish artifacts unless your course requires it; `RunApp/` is normally refreshed locally by the build.

## Disable Demo Mode (final build)

In **`src/Frontend/DesktopBuildOptions.cs`**, set:

```csharp
public const bool EnableDemoMode = false;
```

Rebuild. The **Enter Demo Mode** button is hidden and `NavigateToDemo` does nothing.

See also **`RunApp/README.md`** for student-facing steps.
