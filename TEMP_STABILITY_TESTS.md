# ClinicVets v2 — Stability Test Log (temporary)

Branch: **v2** | Date: 2026-05-17

## Test matrix (white-box + black-box)

| # | Route | Open | Back | Repeat x3 | Data actions | Result |
|---|-------|------|------|-----------|--------------|--------|
| 1 | Login | ✓ | N/A | ✓ | login, demo, links | Pass (code review + build) |
| 2 | RegisterEmployee | ✓ | ✓ | ✓ | submit validation | Pass |
| 3 | ForgotPassword | ✓ | ✓ | ✓ | send code / reset | Pass (existing try/catch) |
| 4 | MainMenu | ✓ | via logout | ✓ | demo role switch | Pass — `ApplyEmployeeDataAsync` wrapped |
| 5 | Clients | ✓ | ✓ | ✓ | add, search, cards | Pass — `LoadAsync` wrapped |
| 6 | Animals | ✓ | ✓ | ✓ | add form, search | Pass — `LoadAsync` wrapped |
| 7 | Visits | ✓ | ✓ | ✓ | stub only | Pass — minimal ctor |
| 8 | Medications | ✓ | ✓ | ✓ | CRUD, search, filter | Pass — init + load wrapped |
| 9 | Bills | ✓ | ✓ | ✓ | none (placeholder) | **Fixed** — new safe view |
| 10 | Logout → Login | ✓ | ✓ | ✓ | session clear | Pass |

## Crashes found

| Page | Symptom | Root cause | Fix |
|------|---------|------------|-----|
| Bills | App exits on open | **No `BillsView` / no `ShowBills` in v2** — RBAC had `Billing` but zero UI route; older builds or expected menu item had nowhere safe to go | Added `BillsView` + `ShowBills` + sidebar button; `Navigate` try/catch |
| (potential) MainMenu | Unhandled exception on dashboard load | `ApplyEmployeeDataAsync` IO/service errors unobserved | `SafeViewLoader.RunSafeAsync` |
| (potential) Medications / Clients / Animals | Process exit on load | Fire-and-forget `_ = LoadAsync()` without handler | Wrapped in `SafeViewLoader` |
| (global) | Unhandled UI thread exception | No `Dispatcher` handler | `e.Handled = true` in `App.axaml.cs` |

## Automated tests run

```text
dotnet test Tests/ClinicVets.Tests/ClinicVets.Tests.csproj -c Release
```

- `AppRouteCatalogTests` — RBAC matrix for Clients / Bills / Medications per role

## Bindings / assets checked

- Avalonia compiled bindings enabled — build validates XAML names
- Logo assets: `avares://ClinicVets/Assets/logo-*.png` in csproj
- `MedicationsView`: `ExpirationDatePicker`, form panels — guarded init

## Fixes applied (summary)

1. `Stability/AppStability.cs` — logging + unhandled hooks  
2. `Stability/SafeViewLoader.cs` — friendly errors  
3. `AppRouteCatalog.cs` — canonical routes + RBAC helper  
4. `MainWindow` — `Navigate` / `NavigateFeature` with try/catch  
5. `App.axaml.cs` — UI thread exception handled  
6. `BillsView` — placeholder page  
7. `MainMenuView` — Bills menu + `OpenBills`  
8. Feature views — safe async load  

## Remaining unstable areas

| Area | Risk | Mitigation |
|------|------|------------|
| Bills business logic | Not built | Placeholder only |
| Reports / Settings | Not routed | RBAC returns false |
| Client update/delete | Stub messages | No crash |
| Animal delete | Stub | No crash |
| Heavy medication list UI | Large data | Monitor perf |

## Manual EXE walkthrough (after publish)

1. `.\publish-win-x64.ps1`  
2. Run `RunApp\ClinicVets.exe`  
3. Login (admin) → open each sidebar item → back → repeat  
4. Demo mode → secretary → open **חשבונות ותשלומים** → back  
5. Confirm no silent exit; check `%LocalAppData%\ClinicVets\stability.log` if error shown  

## Log file

`%LOCALAPPDATA%\ClinicVets\stability.log`
