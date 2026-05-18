# ClinicVets v2 — folder structure

## Frontend (`src/Frontend/`)

| Folder | Purpose |
|--------|---------|
| `Views/Auth/` | Login, register, forgot password |
| `Views/Dashboard/` | Main menu, reports, settings |
| `Views/Customers/` | Clients (לקוחות) |
| `Views/Animals/` | Animals (בעלי חיים) |
| `Views/Visits/` | Visits (ביקורים) |
| `Views/Medicine/` | Medications (תרופות) |
| `Views/Bills/` | Bills placeholder (חשבונות) |
| `Views/Shared/` | Shared placeholders |
| `Services/` | `AppServices`, routes, visit data bridge |
| `Helpers/` | UI helpers, validation, branding |
| `Helpers/Stability/` | Safe navigation / error logging |
| `Styles/` | Global Avalonia styles |
| `Assets/Images/` | Logo resources (linked from `assets/branding/`) |

Shell: `App.axaml`, `MainWindow.axaml`, `Program.cs`

## Backend (`src/Backend/`)

| Project | Purpose |
|---------|---------|
| `ClinicVets.Core` | Entities and domain models |
| `ClinicVets.Application` | Services, security, validation |
| `ClinicVets.Infrastructure` | `Repositories/`, `Demo/`, `Database/` docs |

## Build outputs (not in Git)

- `**/bin/`, `**/obj/`
- `RunApp/` (synced runnable copy; see `RunApp/README.md`)
- `PublishedApp/` (publish output)
