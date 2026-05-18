# Main → v2 page parity report

**Branch:** `v2` (working) vs `origin/main`  
**Date:** 2026-05-17

## Pages / features in `origin/main`

| Area | View / feature | Notes |
|------|----------------|-------|
| Auth | LoginView, RegisterEmployeeView, ForgotPasswordView | SQLite `AppData` |
| Shell | MainMenuView | Sidebar: לקוחות, בעלי חיים, ביקורים, תרופות |
| לקוחות | ClientsView | Full CRUD |
| בעלי חיים | AnimalsView | Chip, weight, vaccination (~700 LOC) |
| ביקורים | VisitsView | Full module (~1400 LOC), SQLite |
| תרופות | MedicationsView | Inventory UI |
| — | EmptyPageView | Placeholder helper |
| — | No Bills / Reports / Settings screens | Not in main navigation |

## Pages / features in `v2` (before this work)

| Area | Status |
|------|--------|
| Auth + Clients + Medications | Present (JSON services, RBAC) |
| בעלי חיים | Simplified AnimalsView |
| ביקורים | **Stub only** (“מודול הביקורים אינו זמין”) |
| חשבונות | Placeholder BillsView (v2-only) |
| דוחות / הגדרות | RBAC enums only, no UI |
| Demo mode | v2-only |

## Missing in v2 (vs main)

1. **ביקורים (Visits)** — full UI and persistence (main’s largest gap).
2. **בעלי חיים** — partial: chip/weight/vaccination fields used by visits not fully exposed in Animals UI.
3. *(Not in main)* חשבונות, דוחות, הגדרות — v2 RBAC expected these routes; needed shell pages.

## Added / restored in v2

### ביקורים (ported from main, adapted)

- Full `VisitsView.axaml` + `VisitsView.axaml.cs` from `origin/main`
- `VisitDataBridge` replaces `AppData` (loads JSON/demo services)
- Backend: `Visit` entity, `IVisitRepository`, `VisitManagementService`, `JsonFileVisitRepository`, `InMemoryVisitRepository`
- `UiFormValidation`: visit date, chip, medication qty, `IsVaccinationDue`
- Safe load via `SafeViewLoader` + `VisitDataBridge.RefreshAsync()` on `Loaded`
- Namespace/bindings: `ClinicVets.Desktop`, `Customer.NationalId`, `DateTimeOffset` for date picker

### דוחות / הגדרות (v2 RBAC placeholders)

- `ReportsView`, `SettingsView` — consistent v2 styling, back navigation
- Sidebar buttons (admin): דוחות, הגדרות
- `AppRouteCatalog` + `MainWindow` routes wired

### Other fixes (same effort)

- Main menu **visit count** from `AppServices.Visits.GetAllAsync()`
- Demo medications + `InMemoryMedicationRepository` (תרופות crash fix)
- Extended `Animal` + customer directory (`GetAllAnimals`, chip lookup) for visits

## Files changed / added

**Frontend**

- `src/Frontend/Views/VisitsView.axaml`, `VisitsView.axaml.cs` (ported + fixed)
- `src/Frontend/VisitDataBridge.cs` (new)
- `src/Frontend/AppServices.cs`, `AppRouteCatalog.cs`, `MainWindow.axaml.cs`
- `src/Frontend/UiFormValidation.cs`
- `src/Frontend/Views/MainMenuView.axaml`, `MainMenuView.axaml.cs`
- `src/Frontend/Views/MedicationsView.axaml.cs`
- `src/Frontend/Views/ReportsView.axaml(.cs)`, `SettingsView.axaml(.cs)` (new)
- `src/Frontend/Views/BillsView.*` (unchanged placeholder)

**Backend**

- `src/Backend/ClinicVets.Core/Entities/Visit.cs` (new)
- `src/Backend/ClinicVets.Core/Models/Animal.cs`
- `src/Backend/ClinicVets.Application/Interfaces/IVisitRepository.cs`, `ICustomerDirectoryRepository.cs`
- `src/Backend/ClinicVets.Application/Services/VisitManagementService.cs` (new)
- `src/Backend/ClinicVets.Infrastructure/Data/JsonFileVisitRepository.cs`, `InMemoryVisitRepository.cs`, `InMemoryMedicationRepository.cs`
- `src/Backend/ClinicVets.Infrastructure/Data/*CustomerDirectory*`, `Demo/DemoWorkspace.cs`

**Tests**

- `Tests/ClinicVets.Tests/Functional/DemoModeMedicationServiceTests.cs` (new)
- `Tests/ClinicVets.Tests/Integration/FakeCustomerDirectoryRepository.cs`
- `Tests/ClinicVets.Tests/Navigation/AppRouteCatalogTests.cs`

## Logic added

- Visit CRUD/list/filter UI bound to in-memory bridge → JSON file or demo in-memory store
- Medication stock updates from visit screen via existing `MedicationInventoryService`
- Animal lookup by chip for visit scheduling
- RBAC routes for Reports (admin) and Settings (admin)

## Tests performed

| Check | Result |
|-------|--------|
| `dotnet build src/Frontend/ClinicVets.Desktop.csproj -c Release` | **Pass** (3 existing warnings) |
| `dotnet test Tests/ClinicVets.Tests -c Release` | **87/87 pass** |
| `publish-win-x64.ps1` → `PublishedApp/`, `RunApp/` | **Pass** |
| `PublishedApp/ClinicVets.exe` launch (3s) | **Starts, no immediate crash** |
| Manual UI walk (every button/form) | **Not automated** — recommend demo login + open each sidebar item |

## Remaining issues

1. **חשבונות (Bills)** — still placeholder; not in `main`; billing workflow not implemented.
2. **דוחות / הגדרות** — placeholder copy only (RBAC satisfied, no reports engine).
3. **בעלי חיים** — AnimalsView still simpler than main; visits use extended `Animal` model but UI may not edit all fields.
4. **Demo visits** — demo mode seeds medications/customers; visit list starts empty until user creates visits.
5. **Treatments** route — `AppRouteCatalog.Treatments` still unimplemented (merged into Visits in UI).
6. **Manual regression** — full click-through of Visits dialogs (add/edit/arrival/medication) should be done on a real session.

## Summary

| Metric | Value |
|--------|--------|
| Missing from main (critical) | **Visits module** |
| Restored | **VisitsView** + visit backend |
| v2-only shells added | Reports, Settings (navigation) |
| Build | OK |
| EXE | OK (starts) |
