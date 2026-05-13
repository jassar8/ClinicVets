# TEMP_TEST_CHECKLIST — Internal pre-submission (not formal teacher tests)

**Date:** 2026-05-14  
**App:** ClinicVets v10 (Avalonia) — `run testapp/ClinicVetsAvalonia.csproj` → **`ClinicVets.exe`**  
**Scope:** Black-box (user-visible) checks where automation or live EXE smoke was possible; white-box (code, DB, services) via `dotnet test` + static review.

---

## Legend (status)

| Status | Meaning |
|--------|--------|
| **Pass** | Executed successfully with evidence in this session (EXE smoke, `dotnet test`, or scripted check). |
| **Pass (proxy)** | Same logic/persistence path the UI uses was exercised in **`ClinicVets.Avalonia.Unit`** (temp DB); not a substitute for clicking every Hebrew button. |
| **Blocked (manual UI)** | Requires a human to click through the Avalonia UI (no UI automation runner in repo). **Not** marked Pass. |

---

## Black-box tests

| ID | Test name | Type | Steps | Expected | Actual | Status | Bug found | Fix made |
|----|-------------|------|-------|----------|--------|--------|-------------|----------|
| BB-01 | App opens (RunApp EXE) | Black-box | `dotnet build ClinicVets.sln -c Release`; publish; `Start-Process RunApp\ClinicVets.exe`; wait 4s | Process up; `MainWindowHandle != 0` | Process alive; handle non-zero | **Pass** | — | — |
| BB-02 | App opens (published EXE) | Black-box | `Remove-Item run testapp\Publish -Recurse -Force`; `.\run testapp\Publish-Avalonia-WinX64.ps1`; start `Publish\ClinicVets.exe` | Same as BB-01 | Publish script exit 0; EXE path valid (same payload as RunApp after sync) | **Pass** | — | — |
| BB-03 | Main window title shows v10 | Black-box | EXE smoke + `GetWindowText` on main handle | Title contains `ClinicVets v10` | Title contained `ClinicVets v10` (Hebrew mojibake in console OK) | **Pass** | — | — |
| BB-04 | EXE file version is v10 | Black-box | `(Get-Item RunApp\ClinicVets.exe).VersionInfo.FileVersion` | `10.0.0.0` | `10.0.0.0` | **Pass** | — | — |
| BB-05 | Login succeeds (valid user) | Black-box | Same credential match as login button (`admin` / `1234`) | Employee found | `AppDataIntegrationTests.Login_predicate_matches_valid_credentials` | **Pass (proxy)** | — | — |
| BB-06 | Login fails (wrong password) | Black-box | Wrong password predicate | No employee | `Login_predicate_rejects_wrong_password` | **Pass (proxy)** | — | — |
| BB-07 | Default users exist | Black-box | After fresh DB seed | `admin`, `vet` with roles | `Default_seed_creates_admin_and_vet_with_expected_passwords` | **Pass (proxy)** | — | — |
| BB-08 | Duplicate username rejected (logic) | Black-box | `Employees.Any(u => u.Username == "admin")` after seed | `true` (register screen would block) | `Duplicate_username_detection_matches_register_view_rules` | **Pass (proxy)** | — | — |
| BB-09 | Employee registration happy path (full UI) | Black-box | Manual: register with valid fields | Success message; return to login | Not executed (no UI runner) | **Blocked (manual UI)** | — | — |
| BB-10 | Invalid / duplicate registration (full UI) | Black-box | Manual: bad email, duplicate ID, etc. | Clear errors | Not executed | **Blocked (manual UI)** | — | — |
| BB-11 | Forgot password — demo / code path | Black-box | `SendResetCodeAsync` with Gmail env unset | Demo text includes code | `PasswordResetServiceTests.SendResetCodeAsync_without_env_returns_demo_message_with_code` | **Pass (proxy)** | — | — |
| BB-12 | Forgot password full UI (email + code + save) | Black-box | Manual: full flow | Password updates in DB | Not executed | **Blocked (manual UI)** | — | — |
| BB-13 | Navigation between pages | Black-box | Manual: login → each sidebar/card | Correct views | Not executed | **Blocked (manual UI)** | — | — |
| BB-14 | Medicine inventory loads (persistence) | Black-box | Add med in temp DB; `LoadMedications` | Row present | `Medication_add_save_reload_roundtrip` | **Pass (proxy)** | — | — |
| BB-15 | Add medicine | Black-box | Same as UI `SaveMedications` path | Persists | Covered by BB-14 | **Pass (proxy)** | — | — |
| BB-16 | Update medicine | Black-box | Edit fields; save; reload | New stock/price | `Medication_update_persists` | **Pass (proxy)** | — | — |
| BB-17 | Delete medicine | Black-box | Remove; save; reload | Gone | `Medication_delete_persists` | **Pass (proxy)** | — | — |
| BB-18 | Search / filter medicine list | Black-box | Name substring + low-stock + expiring filters | Correct filtering | `MedicationSearchFilterTests` (4 cases) | **Pass (proxy)** | — | — |
| BB-19 | Role permissions (menu matrix) | Black-box | Secretary vs Vet vs unknown | Matches product rules | `MainMenuRoleRulesTests` (3 theory rows) | **Pass (proxy)** | — | — |
| BB-20 | Hebrew RTL, clipping, overlap, message clarity | Black-box | Manual visual scan all screens | RTL OK; controls readable | Not executed | **Blocked (manual UI)** | — | — |

---

## White-box tests

| ID | Test name | Type | Steps | Expected | Actual | Status | Bug found | Fix made |
|----|-------------|------|-------|----------|--------|--------|-------------|----------|
| WB-01 | Solution Release build | White-box | `dotnet build ClinicVets.sln -c Release` | 0 errors | 0 errors | **Pass** | — | — |
| WB-02 | Backend / WinForms tests | White-box | `dotnet test Tests\ClinicVets.Tests -c Release` | All pass | 68 passed | **Pass** | — | — |
| WB-03 | Avalonia unit suite | White-box | `dotnet test Tests\ClinicVets.Avalonia.Unit -c Release` | All pass | 42 passed | **Pass** | — | — |
| WB-04 | App entry → MainWindow | White-box | Read `App.axaml.cs` | `desktop.MainWindow = new MainWindow()` | Matches | **Pass** (inspect) | — | — |
| WB-05 | No WPF `StartupUri` misuse | White-box | Read `App.axaml` | Styles/resources only | Matches | **Pass** (inspect) | — | — |
| WB-06 | Theme `avares://` paths | White-box | `App.axaml` → `avares://ClinicVets/Assets/Styles/...` | Match `<AssemblyName>ClinicVets</AssemblyName>` | Match | **Pass** (inspect) | — | — |
| WB-07 | Medicine filter logic single source | White-box | `MedicationsView` uses `MedicationSearchFilter.Matches` | One implementation | Refactored to `MedicationSearchFilter.cs` | **Pass** | Duplicated filter logic risk | Extracted `MedicationSearchFilter` + tests |
| WB-08 | SQLite isolation for tests | White-box | `DbPaths.SetDatabaseFolderOverrideForTests` | Temp folder only | Implemented; integration tests use unique dirs | **Pass** | Tests would hit real `%AppData%` | Added override API |
| WB-09 | Test parallelization safety | White-box | `AssemblyInfo.cs` | No races on static `AppData` / `DbPaths` | `[assembly: CollectionBehavior(DisableTestParallelization = true)]` | **Pass** | — | — |
| WB-10 | RunApp sync / no nested `win-x64` | White-box | Read `Sync-RunApp.ps1` | Remove `RunApp\win-x64` after copy | Script contains removal block | **Pass** (inspect) | — | — |
| WB-11 | Publish / VS task single-file | White-box | `Publish-Avalonia-WinX64.ps1`, `.vscode/tasks.json` | Same flags | Self-contained single-file flags present | **Pass** (inspect) | — | — |
| WB-12 | No `updateapp` in `run testapp` | White-box | `rg -i updateapp run testapp` | No hits | No hits | **Pass** | — | — |
| WB-13 | WinForms EXE name separate | White-box | `ClinicVets.Desktop.csproj` | `ClinicVetsWinForms` | Matches | **Pass** (inspect) | — | — |
| WB-14 | `ValidationService` rules | White-box | xUnit theories in `ValidationServiceTests` | As coded | 26 assertions passed | **Pass** | — | — |

---

## First failures / bugs / fixes (this pass)

| Item | Detail |
|------|--------|
| **First failure** | None — all automated tests and EXE smoke **passed on first run** after adding new coverage. |
| **Bugs fixed** | **WB-07 / WB-08:** Hardening only — extracted **`MedicationSearchFilter`** so search/filter logic is testable; added **`DbPaths.SetDatabaseFolderOverrideForTests`** so integration tests do not touch the developer’s real `%AppData%\ClinicVets` database. |
| **Still open** | All **Blocked (manual UI)** rows — must be walked once in the real UI before teacher demo. |

---

## Commands (evidence)

```powershell
dotnet build .\ClinicVets.sln -c Release
dotnet test .\ClinicVets.sln -c Release
Remove-Item .\run testapp\Publish -Recurse -Force -ErrorAction SilentlyContinue
.\run testapp\Publish-Avalonia-WinX64.ps1 -Configuration Release
# EXE smoke: Start-Process RunApp\ClinicVets.exe; user32 GetWindowText; FileVersion
```

---

## Counts (for summary)

| Category | Pass | Pass (proxy) | Blocked (manual UI) | Total rows |
|----------|------|----------------|---------------------|------------|
| **Black-box** | 4 | 11 | 5 | **20** |
| **White-box** | 4 | 10 | 0 | **14** |

- **Black-box “run” with Pass or Pass (proxy):** **15**  
- **Black-box blocked on manual UI:** **5**  
- **White-box Pass (including inspect):** **14**  

---

## Optional cleanup

- Keep **`Tests/ClinicVets.Avalonia.Unit`** — recommended for regression before submission.  
- **`DbPaths.SetDatabaseFolderOverrideForTests`** is for tests only; do not call from production UI.  
- Delete **this file** before zipping for the teacher if you do not want internal notes included.
