# TEMP_TEST_CHECKLIST — Internal pre-submission (not formal teacher tests)

**Date:** 2026-05-14  
**App:** ClinicVets v4 (Avalonia) — `run testapp/ClinicVetsAvalonia.csproj` → **`ClinicVets.exe`**  
**Note:** GUI flows below that require clicking Hebrew UI were **not executed by an automated UI runner** in this pass. They are marked **Manual required** so they are not falsely marked Pass.

---

## Legend

| Status | Meaning |
|--------|---------|
| **Pass** | Executed here with evidence (build, test run, process smoke, or `dotnet test`). |
| **Pass (automated)** | `ClinicVets.Avalonia.Unit` xUnit theory/fact. |
| **Pass (solution tests)** | Existing `ClinicVets.Tests` (WinForms/backend layer). |
| **Inspect OK** | Static code / grep review; no runtime proof. |
| **Manual required** | You must run in Visual Studio / double-click EXE and follow steps. |

---

## Black-box tests

| ID | Test name | Steps | Expected | Actual | Status | Bug found | Fix made |
|----|-------------|-------|----------|--------|--------|-------------|----------|
| BB-01 | App opens (RunApp EXE) | `dotnet build ClinicVets.sln -c Release`; start `RunApp\ClinicVets.exe`; wait 4s | Process stays up; main window handle non-zero | Process alive; `MainWindowHandle != 0` | **Pass** | — | — |
| BB-02 | App opens (published EXE) | After `Publish-Avalonia-WinX64.ps1`, start `run testapp\Publish\ClinicVets.exe`; wait 4s | Same as BB-01 | Same | **Pass** | — | — |
| BB-03 | Login valid user | Open app; user `admin`, password `1234`; click התחברות | Main menu opens | — | **Manual required** | — | — |
| BB-04 | Login wrong password | Wrong password; click התחברות | Error message; stay on login | — | **Manual required** | — | — |
| BB-05 | Employee registration happy path | Register; valid fields; save | Success message; return to login | — | **Manual required** | — | — |
| BB-06 | Duplicate username | Register with existing username | Clear error | — | **Manual required** | — | — |
| BB-07 | Invalid fields | Invalid email / ID / employee # | Validation errors | — | **Manual required** | — | — |
| BB-08 | Forgot password flow | Send code (or demo message); reset | Success path per UX | — | **Manual required** | — | — |
| BB-09 | Navigation clients | As secretary; open לקוחות | Clients screen | — | **Manual required** | — | — |
| BB-10 | Navigation animals | Open בעלי חיים | Animals screen | — | **Manual required** | — | — |
| BB-11 | Navigation visits | As vet; open ביקורים | Visits screen | — | **Manual required** | — | — |
| BB-12 | Navigation medicines | As vet; open תרופות | Medications screen | — | **Manual required** | — | — |
| BB-13 | Vet cannot open clients | As vet; sidebar clients disabled | Cannot reach clients via disabled UI | Code disables buttons | **Inspect OK** (see `MainMenuView.ApplyEmployeeData`) | — | — |
| BB-14 | Medicine add | Add new medicine | Appears in list / DB | — | **Manual required** | — | — |
| BB-15 | Medicine update | Change stock/price; save | Persists | — | **Manual required** | — | — |
| BB-16 | Medicine delete | Delete selected | Removed | — | **Manual required** | — | — |
| BB-17 | Medicine search / filter | Type search; filter dropdown | List filters | — | **Manual required** | — | — |
| BB-18 | RTL / readability | Visual scan all screens | Hebrew RTL; no clipped primary actions | — | **Manual required** | — | — |
| BB-19 | EXE is v4 Avalonia | Task Manager / file size / dependency folder | Single-file ~100MB OR Avalonia DLLs next to dev EXE; not WinForms-only tiny shell | RunApp after publish: single-file `ClinicVets.exe` present; smoke launch OK | **Pass** (smoke + build sync from `run testapp` only) | — | — |

**Black-box counts:** 19 rows — **4 Pass** (BB-01, BB-02, BB-13 inspect, BB-19), **1 Inspect OK**, **14 Manual required**.

---

## White-box tests

| ID | Test name | Steps | Expected | Actual | Status | Bug found | Fix made |
|----|-------------|-------|----------|--------|--------|-------------|----------|
| WB-01 | Solution Release build | `dotnet build ClinicVets.sln -c Release` | 0 errors | 0 errors | **Pass** | — | — |
| WB-02 | Backend unit tests | `dotnet test Tests\ClinicVets.Tests -c Release` | All pass | 68 passed | **Pass (solution tests)** | — | — |
| WB-03 | Avalonia `ValidationService` | `dotnet test Tests\ClinicVets.Avalonia.Unit -c Release` | All pass | 26 passed | **Pass (automated)** | — | — |
| WB-04 | App entry → MainWindow | Read `App.axaml.cs` `OnFrameworkInitializationCompleted` | `desktop.MainWindow = new MainWindow()` | Matches | **Inspect OK** | — | — |
| WB-05 | No StartupUri misuse | Avalonia has no WPF `StartupUri` | N/A / N/A | `App.axaml` uses styles + resources only | **Inspect OK** | — | — |
| WB-06 | Resource URIs match assembly | `App.axaml` `avares://ClinicVets/...` | Matches `<AssemblyName>ClinicVets</AssemblyName>` | Match | **Inspect OK** | — | — |
| WB-07 | RunApp sync source | `ClinicVetsAvalonia.csproj` AfterTargets | Sync from `run testapp` output only | `Sync-RunApp.ps1` from Avalonia `TargetDir` / `PublishDir` | **Inspect OK** | — | — |
| WB-08 | WinForms not overwriting RunApp | `ClinicVets.Desktop.csproj` | No `SyncRunAppAfterBuild` | Targets removed | **Inspect OK** | — | — |
| WB-09 | No `updateapp` strings in `run testapp` | `rg -i updateapp run testapp` | No hits | Only README line mentioning WinForms | **Inspect OK** | — | — |
| WB-10 | MainWindow navigation wiring | Read `MainWindow.axaml.cs` | Events → `ShowClients` / `ShowMedications` / etc. | All wired | **Inspect OK** | — | — |
| WB-11 | Role gating | `MainMenuView` | Secretary vs Vet button enables | Implemented | **Inspect OK** | — | — |
| WB-12 | SQLite path | `DbPaths` | Uses `%AppData%\ClinicVets\` | `ClinicVets` folder name | **Inspect OK** | — | — |

**White-box counts:** 12 rows — **3 Pass** (build + two test suites), **9 Inspect OK**, **0 Fail**.

---

## First failures / bugs / fixes (this pass)

| Item | Detail |
|------|--------|
| **First failure** | None — automated tests and smoke launches **passed on first run**. |
| **Bugs fixed during checklist** | None required for automated slice. |
| **Follow-up** | Complete all **Manual required** rows before teacher demo. |

---

## Commands used (evidence)

```powershell
dotnet build .\ClinicVets.sln -c Release
dotnet test .\Tests\ClinicVets.Tests\ClinicVets.Tests.csproj -c Release
dotnet test .\Tests\ClinicVets.Avalonia.Unit\ClinicVets.Avalonia.Unit.csproj -c Release
.\run testapp\Publish-Avalonia-WinX64.ps1
# PowerShell: Start-Process RunApp\ClinicVets.exe and run testapp\Publish\ClinicVets.exe — process alive + MainWindowHandle
```

---

## Optional cleanup

- **`Tests/ClinicVets.Avalonia.Unit/`** — small **temporary** automated suite for `ValidationService`; delete if you do not want it in the repo (or keep for regression).
- **This file** — delete before zip for teacher if you do not want internal notes included.
